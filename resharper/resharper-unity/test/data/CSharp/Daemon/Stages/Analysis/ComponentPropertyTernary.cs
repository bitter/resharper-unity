using UnityEngine;

public class Test : MonoBehavior
{
    public void Method(GameObject go)
    {
        var dir = true ? go.transform.position - transform.position : transform.position - go.transform.position;
    }
}
