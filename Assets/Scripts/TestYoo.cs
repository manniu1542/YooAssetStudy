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
        // ��ʼ����Դϵͳ
        YooAssets.Initialize();

        // ����Ĭ�ϵ���Դ��
        var package = YooAssets.CreatePackage("DefaultPackage");

        // ���ø���Դ��ΪĬ�ϵ���Դ��������ʹ��YooAssets��ؼ��ؽӿڼ��ظ���Դ�����ݡ�
        YooAssets.SetDefaultPackage(package);

        //StartCoroutine(InitializeYooAssetEditor(package, () =>
        //{
        //    var go = YooAssets.LoadAssetSync<GameObject>("GameObject");
        //    GameObject.Instantiate(go.GetAssetObject<GameObject>());
        //    sw.Stop();
        //    UnityEngine.Debug.Log("��ʼ����ɣ�" + sw.Elapsed.TotalSeconds);
        //}));

        //StartCoroutine(InitializeYooAssetOffline(package, () =>
        //{
        //    var go = YooAssets.LoadAssetSync<GameObject>("GameObject");
        //    GameObject.Instantiate(go.GetAssetObject<GameObject>());
        //    sw.Stop();
        //    UnityEngine.Debug.Log("��ʼ����ɣ�" + sw.Elapsed.TotalSeconds);
        //}));
        StartCoroutine(InitializeYooAssetNet(package, () =>
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


    /// <summary>
    /// ����ģʽ  ��Դ ��Ҫ��� StreamingAsset  Ŀ¼��
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
    //�༭��ģʽ�µģ�����Ҫ��Դ ��  ֻ��Ҫ���ã��ᶯ̬���ɣ�����    // ����ģʽ�� ��ģ��ģʽ  �������� 
    private IEnumerator InitializeYooAssetEditor(ResourcePackage package, Action act)
    {

        var initParameters = new EditorSimulateModeParameters();
        //���� �༭���µ��嵥 ������Ѿ������ȶ�����Դ ��������� �����ظ�����������Ŀ ���ٹ����嵥������ ��ֱ�� ָ���嵥·����
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
    /// Զ����Դ��ַ��ѯ������
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
    /// Զ����Դ��ַ��ѯ������
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


    //Զ�� ��� ��
    private IEnumerator InitializeYooAssetNet(ResourcePackage package, Action act)
    {
        // ע�⣺GameQueryServices.cs ̫��ս���Ľű��࣬��ϸ��StreamingAssetsHelper.cs
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
            UnityEngine.Debug.Log("��Դ����ʼ���ɹ���");
        }
        else
        {
            UnityEngine.Debug.LogError($"��Դ����ʼ��ʧ�ܣ�{initOperation.Error}");
        }
        act?.Invoke();
    }
}
