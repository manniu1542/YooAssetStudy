using System;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.IO;
using UnityEditor;
using UnityEngine;
using YooAsset;

public class TestYoo : MonoBehaviour
{
    private void Start()
    {
        var sw = Stopwatch.StartNew();
        // 初始化资源系统
        YooAssets.Initialize();

        // 创建默认的资源包
        var package = YooAssets.CreatePackage("DefaultPackage");

        // 设置该资源包为默认的资源包，可以使用YooAssets相关加载接口加载该资源包内容。
        YooAssets.SetDefaultPackage(package);

        //StartCoroutine(InitializeYooAssetEditor(package, () =>
        //{
        //    var go = YooAssets.LoadAssetSync<GameObject>("GameObject");
        //    GameObject.Instantiate(go.GetAssetObject<GameObject>());
        //    sw.Stop();
        //    UnityEngine.Debug.Log("初始化完成！" + sw.Elapsed.TotalSeconds);
        //}));

        //StartCoroutine(InitializeYooAssetOffline(package, () =>
        //{
        //    var go = YooAssets.LoadAssetSync<GameObject>("GameObject");
        //    GameObject.Instantiate(go.GetAssetObject<GameObject>());
        //    sw.Stop();
        //    UnityEngine.Debug.Log("初始化完成！" + sw.Elapsed.TotalSeconds);
        //}));
        StartCoroutine(InitializeYooAssetNet(package, () =>
        {
            var go = YooAssets.LoadAssetSync<GameObject>("GameObject");
            GameObject.Instantiate(go.GetAssetObject<GameObject>());
            sw.Stop();
            UnityEngine.Debug.Log("初始化完成！" + sw.Elapsed.TotalSeconds);
        }));

        sw.Stop();


        UnityEngine.Debug.Log("初始化完成！" + sw.Elapsed.TotalSeconds);
        sw.Restart();
    }


    /// <summary>
    /// 离线模式  资源 需要存放 StreamingAsset  目录下
    /// </summary>
    /// <param name="package"></param>
    /// <param name="act"></param>
    /// <returns></returns>
    private IEnumerator InitializeYooAssetOffline(ResourcePackage package, Action act)
    {
        var initParameters = new OfflinePlayModeParameters();

        yield return package.InitializeAsync(initParameters);

        act?.Invoke();
    }
    //编辑器模式下的，不需要资源 ，  只需要配置（会动态生成）就行    // 离线模式。 ，模拟模式  正常运行 
    private IEnumerator InitializeYooAssetEditor(ResourcePackage package, Action act)
    {

        var initParameters = new EditorSimulateModeParameters();
        //构建 编辑器下的清单 （如果已经有了稳定的资源 情况，可以 不用重复构建，让项目 减少构建清单的运行 ，直接 指定清单路径）
        bool isHaveManifest = true;
        if (!isHaveManifest)
        {
            var simulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild(EDefaultBuildPipeline.BuiltinBuildPipeline, "DefaultPackage");
            UnityEngine.Debug.Log(simulateManifestFilePath);
            initParameters.SimulateManifestFilePath = simulateManifestFilePath;
        }
        else
        {
            initParameters.SimulateManifestFilePath = "E:/UnityProgram/YooAssetStudy/Bundles/StandaloneWindows64/DefaultPackage/Simulate/yooPackageManifest_DefaultPackage_Simulate.bytes";
        }


        yield return package.InitializeAsync(initParameters);


        act?.Invoke();


    }
    /// <summary>
    /// 远端资源地址查询服务类
    /// </summary>
    private class RemoteServices : IRemoteServices
    {
        private readonly string _defaultHostServer;
        private readonly string _fallbackHostServer;

        public RemoteServices(string defaultHostServer, string fallbackHostServer)
        {
            _defaultHostServer = defaultHostServer;
            _fallbackHostServer = fallbackHostServer;
        }
        string IRemoteServices.GetRemoteMainURL(string fileName)
        {
            return $"{_defaultHostServer}/{fileName}";
        }
        string IRemoteServices.GetRemoteFallbackURL(string fileName)
        {
            return $"{_fallbackHostServer}/{fileName}";
        }
    }
    /// <summary>
    /// 远端资源地址查询服务类
    /// </summary>
    private class GameQueryServices : IBuildinQueryServices
    {
        public static bool CompareFileCRC = false;

        public bool Query(string packageName, string fileName, string fileCRC)
        {
            string filePath = Path.Combine(Application.streamingAssetsPath, "yoo", packageName, fileName);
            if (File.Exists(filePath))
            {
                if (GameQueryServices.CompareFileCRC)
                {
                    string crc32 = YooAsset.Editor.EditorTools.GetFileCRC32(filePath);
                    return crc32 == fileCRC;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }


    }


    //远端 存放 包
    private IEnumerator InitializeYooAssetNet(ResourcePackage package, Action act)
    {
        // 注意：GameQueryServices.cs 太空战机的脚本类，详细见StreamingAssetsHelper.cs
        string defaultHostServer = "http://192.168.0.103:8080/BuidIos";
        string fallbackHostServer = "http://192.168.0.103:8080/BuidIos/yoo/DefaultPackage";
        var initParameters = new HostPlayModeParameters();
        initParameters.BuildinQueryServices = new GameQueryServices();
        //initParameters.DecryptionServices = new FileOffsetDecryption();
        initParameters.RemoteServices = new RemoteServices(defaultHostServer, fallbackHostServer);
        var initOperation = package.InitializeAsync(initParameters);
        yield return initOperation;

        if (initOperation.Status == EOperationStatus.Succeed)
        {
            UnityEngine.Debug.Log("资源包初始化成功！");
        }
        else
        {
            UnityEngine.Debug.LogError($"资源包初始化失败：{initOperation.Error}");
        }
        act?.Invoke();
    }
}
