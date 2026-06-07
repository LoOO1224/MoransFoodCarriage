// =============================================================================
// OO_MFC ??븷 二쇱꽍
// - ?ㅽ겕由쏀듃: OOTechCookingManager.cs
// - ??븷: ?붾━ ?쒖뒪?쒖쓽 ?낅젰, 議곕━?꾧뎄, ?덉떆???먯젙???대떦?섎뒗 ?ㅽ겕由쏀듃?낅땲??
// - 媛먮룆 愿?? 遺???λ㈃?먯꽌 ?щ즺? 議곕━?꾧뎄 諛곗슦媛 ?대뼡 ?쒖꽌濡?留뚮굹?붿? 愿由ы빀?덈떎.
// - ?좎?蹂댁닔 ?ъ씤?? ?щ즺 洹쒖튃? ?곗씠?곗? DropTarget ??븷?쒕줈 鍮쇨퀬, UI 諛곗튂??CookingUIGroup?먯꽌 吏곸젒 ?섏젙?⑸땲??
// =============================================================================
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ?붾━ ?덉떆???먯젙???대떦?섎뒗 留ㅻ땲??낅땲??
/// Game View?먯꽌??媛留덉넡???ㅼ뼱???щ즺 議고빀???대뼡 ?뚯떇?쇰줈 ?꾩꽦?섎뒗吏 寃곗젙?⑸땲??
/// </summary>
public class OOTechCookingManager : MonoBehaviour
{
    public static OOTechCookingManager Inst { get; private set; }

    /// <summary>
    /// ?섎굹留?議댁옱?섎뒗 ?붾━ 留ㅻ땲?濡??깅줉?⑸땲??
    /// </summary>
    private void Awake()
    {
        if (Inst != null && Inst != this)
        {
            Destroy(gameObject);
            return;
        }

        Inst = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// ?ъ엯???щ즺 ID 紐⑸줉??Recipe ?곗씠?곗? 鍮꾧탳???붾━ ?깃났/?ㅽ뙣 寃곌낵瑜?諛섑솚?⑸땲??
    /// </summary>
    public CookingResult TryCook(List<string> ingredientIds)
    {
        Debug.Log("[OOTechCookingManager] ?붾━ ?쒕룄");

        if (ingredientIds == null || ingredientIds.Count == 0)
        {
            return new CookingResult
            {
                IsSuccess = false,
                ResultItemId = null,
                FailReason = "?щ즺媛 鍮꾩뼱 ?덉쓬"
            };
        }

        OO_Recipe recipeData = OOTechGameDataManager.Inst != null ? OOTechGameDataManager.Inst.RequestRecipeByIngredientList(ingredientIds) : null;

        if (recipeData != null && !string.IsNullOrEmpty(recipeData.ResultItemId))
        {
            return new CookingResult
            {
                IsSuccess = true,
                ResultItemId = recipeData.ResultItemId,
                FailReason = string.Empty
            };
        }

        if (IsFallbackPumpkinPorridgeRecipe(ingredientIds))
        {
            return new CookingResult
            {
                IsSuccess = true,
                ResultItemId = "OO_PumpkinSoup_1",
                FailReason = string.Empty
            };
        }

        if (IsFallbackVegetablePorridgeRecipe(ingredientIds))
        {
            return new CookingResult
            {
                IsSuccess = true,
                ResultItemId = "OO_VegetableSoup_1",
                FailReason = string.Empty
            };
        }

        if (IsFallbackKimchiStewRecipe(ingredientIds))
        {
            return new CookingResult
            {
                IsSuccess = true,
                ResultItemId = "OO_KimchiStew_1",
                FailReason = string.Empty
            };
        }

        if (IsFallbackKoreanCakeRecipe(ingredientIds))
        {
            return new CookingResult
            {
                IsSuccess = true,
                ResultItemId = "OO_KoreanCake_1",
                FailReason = string.Empty
            };
        }

        if (IsFallbackCarrotStarchRecipe(ingredientIds))
        {
            return new CookingResult
            {
                IsSuccess = true,
                ResultItemId = "OO_CarrotStarch_1",
                FailReason = string.Empty
            };
        }

        if (IsFallbackCarrotCakeRecipe(ingredientIds))
        {
            return new CookingResult
            {
                IsSuccess = true,
                ResultItemId = "OO_CarrotCake_1",
                FailReason = string.Empty
            };
        }

        return new CookingResult
        {
            IsSuccess = false,
            ResultItemId = null,
            FailReason = "留욌뒗 ?덉떆?쇨? ?놁쓬"
        };
    }

    /// <summary>
    /// ?붾━ ?꾧뎄 ?좉툑 ?щ?瑜??뺤씤?⑸땲?? 吏湲덉? 1李?援ы쁽?대씪 紐⑤뱺 ?꾧뎄瑜??덉슜?⑸땲??
    /// </summary>
    public bool IsToolUnlocked(string toolId)
    {
        return true;
    }

    /// <summary>
    /// Recipe ?곗씠?곌? 鍮꾩뼱 ?덉뼱???쒗넗由ъ뼹 ?쇱콈二쎌? ?뚮젅??媛?ν븯?꾨줉 ?④꺼???덉쟾留앹엯?덈떎.
    /// </summary>
    private bool IsFallbackVegetablePorridgeRecipe(List<string> ingredientIds)
    {
        if (ingredientIds == null || ingredientIds.Count != 2)
            return false;

        return ingredientIds.Contains("Ing_Rice_01") &&
               ingredientIds.Contains("Ing_Veggie_01");
    }

    /// <summary>
    /// Recipe ?곗씠?곌? ?꾩쭅 ???ㅼ뼱?붿뼱??Stage1 ?몃컯二??쒖옉??留됲엳吏 ?딄쾶 ?섎뒗 ?덉쟾留앹엯?덈떎.
    /// ?곗씠?곌? ?꾩꽦?섎㈃ OO_Recipe.json 洹쒖튃???곗꽑 ?곸슜?섍퀬, ??fallback? ?ㅼ뿉??諛쏆퀜 以띾땲??
    /// </summary>
    private bool IsFallbackPumpkinPorridgeRecipe(List<string> ingredientIds)
    {
        if (ingredientIds == null || ingredientIds.Count != 2)
            return false;

        return ingredientIds.Contains("Ing_Rice_01") &&
               ingredientIds.Contains("Ing_Pumpkin_01");
    }

    /// <summary>
    /// Stage2 源移섏컡媛??덉떆?쇨? JSON?먯꽌 鍮좎졇 ?덉뼱???쒖뿰??硫덉텛吏 ?딅룄濡?理쒖냼 ?덉쟾留앹쓣 ?〓땲??
    /// 媛먮룆 鍮꾩쑀濡쒕뒗 ?먯떆?멸? ??쾶 ?꾩갑?대룄 諛곗슦媛 湲곕낯 ?숈꽑留뚰겮? 怨꾩냽 怨듭뿰?섍쾶 ?섎뒗 ?꾩떆 ?먯엯?덈떎.
    /// </summary>
    private bool IsFallbackKimchiStewRecipe(List<string> ingredientIds)
    {
        if (ingredientIds == null || ingredientIds.Count != 2)
            return false;

        return ingredientIds.Contains("Ing_Kimch_01") &&
               ingredientIds.Contains("Ing_ChiliPepper_01");
    }

    /// <summary>
    /// Stage3 ?덇뎄 ?쒗넗由ъ뼹???덉쟾留앹엯?덈떎. OO_Recipe ?곗씠?곌? ??쾶 濡쒕뱶?섏뼱??? 1媛쒕? ?덇뎄???ｌ쑝硫????꾨낫媛 ?⑸땲??
    /// </summary>
    private bool IsFallbackKoreanCakeRecipe(List<string> ingredientIds)
    {
        if (ingredientIds == null || ingredientIds.Count != 1)
            return false;

        return ingredientIds.Contains("Ing_Rice_01");
    }

    /// <summary>
    /// Stage4 ?밴렐?꾨텇 fallback?낅땲??
    /// ?덉떆???곗씠?곌? ?꾩쭅 而⑤쾭?낅릺吏 ?딆븘???밴렐怨??≪쓣 議고빀?섎㈃ ?ㅼ쓬 ?λ㈃?쇰줈 吏꾪뻾?????덇쾶 ?섎뒗 蹂댄뿕?낅땲??
    /// </summary>
    private bool IsFallbackCarrotStarchRecipe(List<string> ingredientIds)
    {
        if (ingredientIds == null || ingredientIds.Count != 2)
            return false;

        return ingredientIds.Contains("Ing_Carrot_01") &&
               ingredientIds.Contains("OO_KoreanCake_1");
    }

    /// <summary>
    /// Stage4 ?밴렐??fallback?낅땲??
    /// ?밴렐?꾨텇? 媛留덉넡???ｋ뒗 ?⑥씪 ?щ즺 ?붾━???곗씠?곌? 鍮꾩뼱????洹쒖튃?쇰줈 ?꾩꽦 泥섎━?⑸땲??
    /// </summary>
    private bool IsFallbackCarrotCakeRecipe(List<string> ingredientIds)
    {
        if (ingredientIds == null || ingredientIds.Count != 1)
            return false;

        return ingredientIds.Contains("OO_CarrotStarch_1");
    }
}

