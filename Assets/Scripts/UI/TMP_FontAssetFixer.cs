/* Put in  TMP_EditorUtility.cs
private static Material GetSafeMaterial(TMP_FontAsset fontAsset)
    {
        if (fontAsset != null && fontAsset.material != null)
            return fontAsset.material;

        // Load fallback TMP_FontAsset
        TMP_FontAsset fallback = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fonts/Kanit/Kanit-Regular SDF.asset");
        if (fallback != null && fallback.material != null)
        {
            Debug.LogWarning($"⚠️ TMP_FontAsset '{fontAsset?.name}' has no material. Using fallback: {fallback.name}");
            return fallback.material;
        }

        Debug.LogError("❌ Fallback TMP_FontAsset not found or has no material at: Assets/Fonts/Kanit/Kanit-Regular SDF.asset");
        return null;
}
*/