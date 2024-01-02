//自定义扩展范例
using System.IO;
using YooAsset.Editor;


public class TestPakeage: IPackRule
{
    PackRuleResult IPackRule.GetPackRuleResult(PackRuleData data)
    {
        //"Assets/Config/test.txt" --> "Assets/Config"
        string bundleName = Path.GetDirectoryName(data.AssetPath);
        PackRuleResult result = new PackRuleResult(bundleName, DefaultPackRule.AssetBundleFileExtension);
        return result;
    }
    //bool IPackRule.IsRawFilePackRule()
    //{
    //    return false;
    //}
}

public class CollectScene2222 : IFilterRule
{
    public bool IsCollectAsset(FilterRuleData data)
    {
        return Path.GetExtension(data.AssetPath) == ".unity";
    }
}