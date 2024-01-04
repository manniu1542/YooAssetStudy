using System;
using System.Collections;
using System.Diagnostics;
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

        StartCoroutine(InitializeYooAsset(package, () =>
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
    private IEnumerator InitializeYooAsset(ResourcePackage package, Action act)
    {
        var initParameters = new EditorSimulateModeParameters();
        var simulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild(EDefaultBuildPipeline.BuiltinBuildPipeline, "DefaultPackage");
        initParameters.SimulateManifestFilePath = simulateManifestFilePath;
        yield return package.InitializeAsync(initParameters);


        act?.Invoke();


    }

}
