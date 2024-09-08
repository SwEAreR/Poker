using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

public static class Common
{
    public static string BeanCountSet(long count)
    {
        string result = count.ToString();
        if (count <= 9999999)
        {
            return result;
        }
        else /* (count >= 10000000)*/
        {
            return (count / 10000000).ToString();
        }
    }
}

public static class ExtensionsResources
{
    public static ResourceRequestAwaiter GetAwaiter(this ResourceRequest request) =>
        new ResourceRequestAwaiter(request);

    public static async Task<T> LoadResourcesAsync<T>(string path) where T : UnityEngine.Object
    {
        var gres = Resources.LoadAsync(path);
        await gres;
        return gres.asset as T;
    }

}

public class ResourceRequestAwaiter : INotifyCompletion
{
    public Action Continuation;
    public ResourceRequest resourceRequest;
    public bool IsCompleted => resourceRequest.isDone;

    public ResourceRequestAwaiter(ResourceRequest resourceRequest)
    {
        this.resourceRequest = resourceRequest;

        //注册完成时的回调
        this.resourceRequest.completed += Accomplish;
    }

    //awati 后面的代码包装成 continuation ，保存在类中方便完成是调用
    public void OnCompleted(Action continuation) => this.Continuation = continuation;

    public void Accomplish(AsyncOperation asyncOperation) => Continuation?.Invoke();

    public void GetResult()
    {
    }
}