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
        // ��ʼ����Դϵͳ
        YooAssets.Initialize();

        // ����Ĭ�ϵ���Դ��
        var package = YooAssets.CreatePackage("DefaultPackage");

        // ���ø���Դ��ΪĬ�ϵ���Դ��������ʹ��YooAssets��ؼ��ؽӿڼ��ظ���Դ�����ݡ�
        YooAssets.SetDefaultPackage(package);

        StartCoroutine(InitializeYooAsset(package, () =>
        {
            var go = YooAssets.LoadAssetSync<GameObject>("GameObject");
            GameObject.Instantiate(go.GetAssetObject<GameObject>());
            sw.Stop();
            UnityEngine.Debug.Log("��ʼ����ɣ�" + sw.Elapsed.TotalSeconds);
        }));


        sw.Stop();


       UnityEngine.Debug.Log("��ʼ����ɣ�" + sw.Elapsed.TotalSeconds);
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
