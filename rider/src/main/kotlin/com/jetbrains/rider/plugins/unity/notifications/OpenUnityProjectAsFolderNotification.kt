package com.jetbrains.rider.plugins.unity.notifications

import com.intellij.ide.projectView.ProjectView
import com.intellij.notification.*
import com.intellij.openapi.actionSystem.AnActionEvent
import com.intellij.openapi.application.ApplicationInfo
import com.intellij.openapi.project.Project
import com.intellij.openapi.project.ex.ProjectManagerEx
import com.intellij.openapi.rd.util.launchNonUrgentBackground
import com.intellij.openapi.rd.util.withUiContext
import com.intellij.openapi.vfs.VirtualFile
import com.intellij.openapi.wm.impl.welcomeScreen.WelcomeFrame
import com.intellij.util.ui.EdtInvocationManager
import com.intellij.workspaceModel.ide.WorkspaceModel
import com.jetbrains.rd.ide.model.RdExistingSolution
import com.jetbrains.rd.ide.model.RdVirtualSolution
import com.jetbrains.rd.platform.util.idea.ProtocolSubscribedProjectComponent
import com.jetbrains.rd.util.reactive.valueOrDefault
import com.jetbrains.rd.util.reactive.whenTrue
import com.jetbrains.rider.model.*
import com.jetbrains.rider.plugins.unity.UnityProjectDiscoverer
import com.jetbrains.rider.plugins.unity.model.frontendBackend.frontendBackendModel
import com.jetbrains.rider.plugins.unity.explorer.UnityExplorer
import com.jetbrains.rider.plugins.unity.util.EditorInstanceJson
import com.jetbrains.rider.plugins.unity.util.EditorInstanceJsonStatus
import com.jetbrains.rider.plugins.unity.util.UnityInstallationFinder
import com.jetbrains.rider.plugins.unity.workspace.hasPackage
import com.jetbrains.rider.projectDir
import com.jetbrains.rider.projectView.SolutionManager
import com.jetbrains.rider.projectView.solution
import com.jetbrains.rider.projectView.solutionDescription
import com.jetbrains.rider.projectView.workspace.ProjectModelEntity
import com.jetbrains.rider.projectView.workspace.ProjectModelEntityVisitor
import com.jetbrains.rider.projectView.workspace.getSolutionEntity
import javax.swing.event.HyperlinkEvent

class OpenUnityProjectAsFolderNotification(project: Project) : ProtocolSubscribedProjectComponent(project) {

    companion object {
        private val notificationGroupId = NotificationGroupManager.getInstance().getNotificationGroup("Unity project open")
    }

    init {
        project.solution.isLoaded.whenTrue(projectComponentLifetime) {
            if (!UnityProjectDiscoverer.getInstance(project).isUnityProjectFolder)
                return@whenTrue

            val solutionDescription = project.solutionDescription
            val title = "Advanced Unity integration is unavailable"
            val marketingVersion = ApplicationInfo.getInstance().fullVersion
            var content = "Make sure \"JetBrains Rider Editor\" is installed in Unity’s Package Manager and Rider $marketingVersion is set as the External Editor."
            if (solutionDescription is RdExistingSolution) { // proper solution
                it.launchNonUrgentBackground {
                    // Sometimes in Unity "External Script Editor" is set to "Open by file extension"
                    // We check that Library/EditorInstance.json is present, but protocol connection was not initialized
                    // also check that all projects are loaded fine
                    if (EditorInstanceJson.getInstance(project).status == EditorInstanceJsonStatus.Valid
                        && !project.solution.frontendBackendModel.unityEditorConnected.valueOrDefault(false)
                        && !hasUnloadedProjects(project)
                    ) {
                        if (!UnityInstallationFinder.getInstance(project).requiresRiderPackage())
                            content = "Make sure Rider $marketingVersion is set as the External Editor in Unity preferences."
                        val notification = Notification(notificationGroupId.displayId, title, content, NotificationType.WARNING)
                        withUiContext { ->
                            Notifications.Bus.notify(notification, project)
                            project.solution.frontendBackendModel.unityEditorConnected.whenTrue(it) { notification.expire() }
                        }
                    }
                }
            }
            else if (solutionDescription is RdVirtualSolution) { // opened as folder
                val adviceText = "Please <a href=\"close\">close</a> and reopen through the Unity editor, or by opening a .sln file."
                val mainText =
                    if (solutionDescription.projectFilePaths.isEmpty())
                        "C# and Unity specific features are not available when the project is opened as a folder."
                    else
                        "C# and Unity specific features are not available when only a single project is opened."

                // todo: hasPackage is unreliable, when PackageManager is still in progress
                // Revisit this after PackageManager is moved to backend
                // MTE: There is an inherent race condition here. Packages can be updated at any time, so we can't
                // be sure that PackageManager is fully loaded at this time.
                val contentWoSolution =
                    if (UnityInstallationFinder.getInstance(project).requiresRiderPackage()
                        && !WorkspaceModel.getInstance(project).hasPackage("com.unity.ide.rider")
                    ) {
                        """$mainText       
            <ul style="margin-left:10px">
              <li>$adviceText</li>
              <li>$content</li>
            </ul>
            """ } else { "$mainText $adviceText" }

                val notification = Notification(notificationGroupId.displayId, "This looks like a Unity project", contentWoSolution, NotificationType.WARNING)
                notification.setListener { _, hyperlinkEvent ->

                    if (hyperlinkEvent.eventType != HyperlinkEvent.EventType.ACTIVATED) return@setListener

                    if (hyperlinkEvent.description == "close") {
                        ProjectManagerEx.getInstanceEx().closeAndDispose(project)
                        WelcomeFrame.showIfNoProjectOpened()
                    }
                }

                val baseDir: VirtualFile = project.projectDir
                val solutionFile = baseDir.findChild(baseDir.name + ".sln")
                if (solutionFile != null && solutionFile.exists()) {
                    notification.addAction(object : NotificationAction("Reopen as Unity project") {
                        override fun actionPerformed(e: AnActionEvent, n: Notification) {
                            // SolutionManager doesn't close the current project if focusOpenInNewFrame is set to true,
                            // and if it's set to false, we get prompted if we want to open in new or same frame. We
                            // don't care - we want to close this project, so new frame or reusing means nothing
                            e.project?.let { ProjectManagerEx.getInstanceEx().closeAndDispose(it) }
                            val newProject = SolutionManager.openExistingSolution(null, true, solutionFile, true, true) ?: return

                            // Opening as folder saves settings to `.idea/.idea.{folder}`. This includes the last selected
                            // solution view pane, which will be file system. A Unity generated solution will use the
                            // same settings folder, so will read the last selected solution view pane and fail to show
                            // the Unity explorer view. We'll override that saved value here, and make Unity Explorer
                            // the currently selected value. See RIDER-17865
                            EdtInvocationManager.getInstance().invokeLater {
                                val projectView = ProjectView.getInstance(newProject)
                                projectView.changeView(UnityExplorer.ID)
                            }
                        }
                    })
                }

                Notifications.Bus.notify(notification, project)
            }
        }
    }

    private fun hasUnloadedProjects(project: Project): Boolean {
        val visitor = object : ProjectModelEntityVisitor() {
            var unloadedProjects = 0

            override fun visitUnloadedProject(entity: ProjectModelEntity): Result {
                val state = (entity.descriptor as RdUnloadProjectDescriptor).state
                if (state == RdUnloadProjectState.LoadFailed)
                    unloadedProjects++
                return Result.Stop
            }
        }

        val solutionEntity = WorkspaceModel.getInstance(project).getSolutionEntity() ?: return true
        visitor.visit(solutionEntity)

        return visitor.unloadedProjects > 0
    }
}