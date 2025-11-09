// Auto-generated registry (loads JSON generated from XLSX)
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.IO;
using UnityEngine;

public static class GameDataRegistry
{
    public static readonly List<AchievementsRow> AchievementsList = new();
    public static readonly List<ArtifactCostsRow> ArtifactCostsList = new();
    public static readonly List<ArtifactsRow> ArtifactsList = new();
    public static readonly List<BattlePassQuestsRow> BattlePassQuestsList = new();
    public static readonly List<BattlePassRewardsRow> BattlePassRewardsList = new();
    public static readonly List<CombatPowerWeightsRow> CombatPowerWeightsList = new();
    public static readonly List<CostumesRow> CostumesList = new();
    public static readonly List<DiamondDungeonRow> DiamondDungeonList = new();
    public static readonly List<FamesRow> FamesList = new();
    public static readonly List<GoldDungeonRow> GoldDungeonList = new();
    public static readonly List<GuideQuestsRow> GuideQuestsList = new();
    public static readonly List<ItemDropsRow> ItemDropsList = new();
    public static readonly List<Items_EtcRow> Items_EtcList = new();
    public static readonly List<MonstersRow> MonstersList = new();
    public static readonly List<RebornRow> RebornList = new();
    public static readonly List<RebornLevelRow> RebornLevelList = new();
    public static readonly List<ShopCurrencyRow> ShopCurrencyList = new();
    public static readonly List<ShopPackageRow> ShopPackageList = new();
    public static readonly List<SkillsRow> SkillsList = new();
    public static readonly List<StagesRow> StagesList = new();
    public static readonly List<StatGrowthGoldsRow> StatGrowthGoldsList = new();
    public static readonly List<StatRerollGemDungeonRow> StatRerollGemDungeonList = new();
    public static readonly List<StatsRow> StatsList = new();
    public static readonly List<WeaponGachaRow> WeaponGachaList = new();
    public static readonly List<WeaponOptionLockRow> WeaponOptionLockList = new();
    public static readonly List<WeaponOptionsRow> WeaponOptionsList = new();
    public static readonly List<WeaponsRow> WeaponsList = new();
    public static readonly List<WeaponStatRerollRow> WeaponStatRerollList = new();

    static readonly string KEY_PREFIX = null;
    static readonly string JSON_ABS   = "C:/Users/MyCom/Downloads/TabIdleReal/Assets/Resources/CSVDataJson";

    static int GetInt(Dictionary<string, object> m, string k, int def = 0)
    {
        if (m == null || !m.TryGetValue(k, out var v) || v == null) return def;
        try
        {
            if (v is int i) return i;
            if (v is long l) return (int)l;
            var s = v.ToString();
            if (string.IsNullOrWhiteSpace(s)) return def;
            s = s.Replace("\u00A0", string.Empty).Replace(" ", string.Empty);
            if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
                return (int)Math.Round(d);
            return Convert.ToInt32(s, CultureInfo.InvariantCulture);
        }
        catch { return def; }
    }

    static BigNum GetBigNum(Dictionary<string, object> m, string k)
    {
        if (m == null || !m.TryGetValue(k, out var v) || v == null) return BigNum.Zero;
        try
        {
            if (v is int i) return BigNum.FromInt(i);
            if (v is long l) return BigNum.FromLong(l);
            if (v is double d) return BigNum.FromDouble(d);
            if (v is float f) return BigNum.FromDouble(f);
            var s = v.ToString();
            if (string.IsNullOrWhiteSpace(s)) return BigNum.Zero;
            s = s.Replace("\u00A0", string.Empty).Replace(" ", string.Empty).Replace(",", string.Empty);
            if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d2))
                return BigNum.FromDouble(d2);
            return BigNum.Zero;
        }
        catch { return BigNum.Zero; }
    }

    static float GetFloat(Dictionary<string, object> m, string k, float def = 0f)
    {
        if (m == null || !m.TryGetValue(k, out var v) || v == null) return def;
        try
        {
            if (v is float f)   return f;
            if (v is double d)  return (float)d;
            if (v is int i)     return i;
            if (v is long l)    return l;
            var s = v.ToString();
            if (string.IsNullOrWhiteSpace(s)) return def;
            s = s.Replace("\u00A0", "").Replace(" ", "");
            if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var dd))
                return (float)dd;
            return Convert.ToSingle(s, CultureInfo.InvariantCulture);
        }
        catch { return def; }
    }

    static bool   GetBool  (Dictionary<string, object> m, string k, bool def=false){ if(m==null||!m.TryGetValue(k,out var v)) return def; if(v is bool b) return b; if(v is IConvertible c){ var s=c.ToString(CultureInfo.InvariantCulture); if(bool.TryParse(s,out var bb)) return bb; if(s=="1")return true; if(s=="0")return false;} return def; }
    static string GetString(Dictionary<string, object> m, string k, string def=""){ if(m==null||!m.TryGetValue(k,out var v)||v==null) return def; return v.ToString(); }
    static StatType ParseStatType(string s){ if(string.IsNullOrEmpty(s)) return StatType.Unknown; var key=s.Trim().Replace(" ","_").Replace("-","_"); if(!Enum.TryParse<StatType>(key,true,out var e)) e=StatType.Unknown; return e; }

    static class MiniJsonLite
    {
        public static List<Dictionary<string, object>> ParseArray(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;
            int i=0; SkipWs(json, ref i); if(i>=json.Length||json[i]!='[') return null; i++;
            var list = new List<Dictionary<string, object>>();
            while(true){ SkipWs(json, ref i); if(i>=json.Length) break; if(json[i]==']'){ i++; break; } var obj=ParseObject(json, ref i); if(obj==null) break; list.Add(obj); SkipWs(json, ref i); if(i<json.Length && json[i]==','){ i++; continue; } }
            return list;
        }
        static Dictionary<string, object> ParseObject(string s, ref int i)
        {
            SkipWs(s, ref i); if(i>=s.Length||s[i]!='{') return null; i++; var d=new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            while(true){ SkipWs(s, ref i); if(i>=s.Length) break; if(s[i]=='}'){ i++; break; } var key=ParseString(s, ref i); SkipWs(s, ref i); if(i>=s.Length||s[i] != ':') break; i++; var val=ParseValue(s, ref i); d[key]=val; SkipWs(s, ref i); if(i<s.Length && s[i]==','){ i++; continue; } }
            return d;
        }
        static object ParseValue(string s, ref int i)
        {
            SkipWs(s, ref i); if(i>=s.Length) return null; char c=s[i];
            if(c=='"') return ParseString(s, ref i);
            if(c=='{')  return ParseObject(s, ref i);
            if(c=='['){ var arr=new List<object>(); i++; while(true){ SkipWs(s, ref i); if(i>=s.Length) break; if(s[i]==']'){ i++; break; } var v=ParseValue(s, ref i); arr.Add(v); SkipWs(s, ref i); if(i<s.Length && s[i]==','){ i++; continue; } } return arr; }
            int start=i; while(i<s.Length && ",}]\t\r\n ".IndexOf(s[i])<0) i++; var token=s.Substring(start,i-start);
            if(string.Equals(token,"true",StringComparison.OrdinalIgnoreCase)) return true;
            if(string.Equals(token,"false",StringComparison.OrdinalIgnoreCase)) return false;
            if(string.Equals(token,"null",StringComparison.OrdinalIgnoreCase)) return null;
            if(double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var num)) return num;
            return token;
        }
        static string ParseString(string s, ref int i)
        {
            if(i>=s.Length || s[i] != '"') return string.Empty; i++; var sb=new System.Text.StringBuilder();
            while(i<s.Length){ char c=s[i++]; if(c=='"') break; if(c=='\\'){ if(i>=s.Length) break; char e=s[i++]; switch(e){ case 'n': sb.Append('\n'); break; case 'r': break; case 't': sb.Append('\t'); break; case '"': sb.Append('"'); break; case '\\': sb.Append('\\'); break; default: sb.Append(e); break; } } else sb.Append(c);} return sb.ToString();
        }
        static void SkipWs(string s, ref int i){ while(i<s.Length){ char c=s[i]; if(c==' '||c=='\t'||c=='\n'||c=='\r') i++; else break; } }
    }

    static GameDataRegistry()
    {
        Load_Achievements();
        Load_ArtifactCosts();
        Load_Artifacts();
        Load_BattlePassQuests();
        Load_BattlePassRewards();
        Load_CombatPowerWeights();
        Load_Costumes();
        Load_DiamondDungeon();
        Load_Fames();
        Load_GoldDungeon();
        Load_GuideQuests();
        Load_ItemDrops();
        Load_Items_Etc();
        Load_Monsters();
        Load_Reborn();
        Load_RebornLevel();
        Load_ShopCurrency();
        Load_ShopPackage();
        Load_Skills();
        Load_Stages();
        Load_StatGrowthGolds();
        Load_StatRerollGemDungeon();
        Load_Stats();
        Load_WeaponGacha();
        Load_WeaponOptionLock();
        Load_WeaponOptions();
        Load_Weapons();
        Load_WeaponStatReroll();
    }

    static void Load_Achievements()
    {
        string jsonText = null;
        // 1) Resources 우선
        if (!string.IsNullOrEmpty(KEY_PREFIX))
        {
            var ta = Resources.Load<TextAsset>(null);
            if (ta != null) jsonText = ta.text;
        }
#if UNITY_EDITOR
        // 2) 에디터 폴백: 파일 직접 읽기
        if (string.IsNullOrEmpty(jsonText))
        {
            var p = System.IO.Path.Combine(JSON_ABS, "Achievements.json");
            if (System.IO.File.Exists(p))
                jsonText = System.IO.File.ReadAllText(p, new UTF8Encoding(false));
        }
#endif

        if (string.IsNullOrEmpty(jsonText))
        {
            Debug.LogError($"[Registry] JSON not found: {(KEY_PREFIX==null? JSON_ABS : ("Resources/"+KEY_PREFIX))}/Achievements.json");
            return;
        }

        var rows = MiniJsonLite.ParseArray(jsonText);
        if (rows == null) { Debug.LogError("[Registry] JSON parse failed"); return; }
        var list = AchievementsList; list.Clear();
        foreach (var map in rows)
        {
            var d = new AchievementsRow();
            d.ID = GetInt(map, "ID", 0);
            d.Name = GetString(map, "Name", "");
            d.Step = GetInt(map, "Step", 0);
            d.QuestType = GetString(map, "QuestType", "");
            d.AchievementCount = GetInt(map, "AchievementCount", 0);
            d.RewardItem = GetInt(map, "RewardItem", 0);
            d.RewardAmount = GetInt(map, "RewardAmount", 0);
            list.Add(d);
        }
        Debug.Log($"[Registry] Achievements loaded: { AchievementsList.Count } rows");
    }

    static void Load_ArtifactCosts()
    {
        string jsonText = null;
        // 1) Resources 우선
        if (!string.IsNullOrEmpty(KEY_PREFIX))
        {
            var ta = Resources.Load<TextAsset>(null);
            if (ta != null) jsonText = ta.text;
        }
#if UNITY_EDITOR
        // 2) 에디터 폴백: 파일 직접 읽기
        if (string.IsNullOrEmpty(jsonText))
        {
            var p = System.IO.Path.Combine(JSON_ABS, "ArtifactCosts.json");
            if (System.IO.File.Exists(p))
                jsonText = System.IO.File.ReadAllText(p, new UTF8Encoding(false));
        }
#endif

        if (string.IsNullOrEmpty(jsonText))
        {
            Debug.LogError($"[Registry] JSON not found: {(KEY_PREFIX==null? JSON_ABS : ("Resources/"+KEY_PREFIX))}/ArtifactCosts.json");
            return;
        }

        var rows = MiniJsonLite.ParseArray(jsonText);
        if (rows == null) { Debug.LogError("[Registry] JSON parse failed"); return; }
        var list = ArtifactCostsList; list.Clear();
        foreach (var map in rows)
        {
            var d = new ArtifactCostsRow();
            d.ID = GetInt(map, "ID", 0);
            d.ArtifactCount = GetInt(map, "ArtifactCount", 0);
            d.TreasureCount = GetInt(map, "TreasureCount", 0);
            list.Add(d);
        }
        Debug.Log($"[Registry] ArtifactCosts loaded: { ArtifactCostsList.Count } rows");
    }

    static void Load_Artifacts()
    {
        string jsonText = null;
        // 1) Resources 우선
        if (!string.IsNullOrEmpty(KEY_PREFIX))
        {
            var ta = Resources.Load<TextAsset>(null);
            if (ta != null) jsonText = ta.text;
        }
#if UNITY_EDITOR
        // 2) 에디터 폴백: 파일 직접 읽기
        if (string.IsNullOrEmpty(jsonText))
        {
            var p = System.IO.Path.Combine(JSON_ABS, "Artifacts.json");
            if (System.IO.File.Exists(p))
                jsonText = System.IO.File.ReadAllText(p, new UTF8Encoding(false));
        }
#endif

        if (string.IsNullOrEmpty(jsonText))
        {
            Debug.LogError($"[Registry] JSON not found: {(KEY_PREFIX==null? JSON_ABS : ("Resources/"+KEY_PREFIX))}/Artifacts.json");
            return;
        }

        var rows = MiniJsonLite.ParseArray(jsonText);
        if (rows == null) { Debug.LogError("[Registry] JSON parse failed"); return; }
        var list = ArtifactsList; list.Clear();
        foreach (var map in rows)
        {
            var d = new ArtifactsRow();
            d.ID = GetInt(map, "ID", 0);
            d.Name = GetString(map, "Name", "");
            d.Desc = GetString(map, "Desc", "");
            d.Icon = GetString(map, "Icon", "");
            d.costGrowth = GetInt(map, "costGrowth", 0);
            d.maxLevel = GetInt(map, "maxLevel", 0);
            d.damageMultiplier = GetFloat(map, "damageMultiplier", 0f);
            d.damageMultipleGrowth = GetFloat(map, "damageMultipleGrowth", 0f);
            d.statType = ParseStatType(GetString(map, "statType", ""));
            d.Value = GetFloat(map, "Value", 0f);
            d.valueGrowth = GetFloat(map, "valueGrowth", 0f);
            list.Add(d);
        }
        Debug.Log($"[Registry] Artifacts loaded: { ArtifactsList.Count } rows");
    }

    static void Load_BattlePassQuests()
    {
        string jsonText = null;
        // 1) Resources 우선
        if (!string.IsNullOrEmpty(KEY_PREFIX))
        {
            var ta = Resources.Load<TextAsset>(null);
            if (ta != null) jsonText = ta.text;
        }
#if UNITY_EDITOR
        // 2) 에디터 폴백: 파일 직접 읽기
        if (string.IsNullOrEmpty(jsonText))
        {
            var p = System.IO.Path.Combine(JSON_ABS, "BattlePassQuests.json");
            if (System.IO.File.Exists(p))
                jsonText = System.IO.File.ReadAllText(p, new UTF8Encoding(false));
        }
#endif

        if (string.IsNullOrEmpty(jsonText))
        {
            Debug.LogError($"[Registry] JSON not found: {(KEY_PREFIX==null? JSON_ABS : ("Resources/"+KEY_PREFIX))}/BattlePassQuests.json");
            return;
        }

        var rows = MiniJsonLite.ParseArray(jsonText);
        if (rows == null) { Debug.LogError("[Registry] JSON parse failed"); return; }
        var list = BattlePassQuestsList; list.Clear();
        foreach (var map in rows)
        {
            var d = new BattlePassQuestsRow();
            d.ID = GetInt(map, "ID", 0);
            d.Name = GetString(map, "Name", "");
            d.Type = GetString(map, "Type", "");
            d.QuestType = GetString(map, "QuestType", "");
            d.CompleteCount = GetInt(map, "CompleteCount", 0);
            d.RewardPoint = GetInt(map, "RewardPoint", 0);
            list.Add(d);
        }
        Debug.Log($"[Registry] BattlePassQuests loaded: { BattlePassQuestsList.Count } rows");
    }

    static void Load_BattlePassRewards()
    {
        string jsonText = null;
        // 1) Resources 우선
        if (!string.IsNullOrEmpty(KEY_PREFIX))
        {
            var ta = Resources.Load<TextAsset>(null);
            if (ta != null) jsonText = ta.text;
        }
#if UNITY_EDITOR
        // 2) 에디터 폴백: 파일 직접 읽기
        if (string.IsNullOrEmpty(jsonText))
        {
            var p = System.IO.Path.Combine(JSON_ABS, "BattlePassRewards.json");
            if (System.IO.File.Exists(p))
                jsonText = System.IO.File.ReadAllText(p, new UTF8Encoding(false));
        }
#endif

        if (string.IsNullOrEmpty(jsonText))
        {
            Debug.LogError($"[Registry] JSON not found: {(KEY_PREFIX==null? JSON_ABS : ("Resources/"+KEY_PREFIX))}/BattlePassRewards.json");
            return;
        }

        var rows = MiniJsonLite.ParseArray(jsonText);
        if (rows == null) { Debug.LogError("[Registry] JSON parse failed"); return; }
        var list = BattlePassRewardsList; list.Clear();
        foreach (var map in rows)
        {
            var d = new BattlePassRewardsRow();
            d.ID = GetInt(map, "ID", 0);
            d.Season = GetInt(map, "Season", 0);
            d.StartDate = GetString(map, "StartDate", "");
            d.ExpireDate = GetString(map, "ExpireDate", "");
            d.Tier = GetInt(map, "Tier", 0);
            d.Exp = GetInt(map, "Exp", 0);
            d.BuyTierPrice = GetInt(map, "BuyTierPrice", 0);
            d.PremiumRewardItemType = GetString(map, "PremiumRewardItemType", "");
            d.PremiumRewardItemID = GetInt(map, "PremiumRewardItemID", 0);
            d.PremiumRewardItemAmount = GetInt(map, "PremiumRewardItemAmount", 0);
            d.NormalRewardItemType = GetString(map, "NormalRewardItemType", "");
            d.NormalRewardItemID = GetInt(map, "NormalRewardItemID", 0);
            d.NormalRewardItemAmount = GetInt(map, "NormalRewardItemAmount", 0);
            list.Add(d);
        }
        Debug.Log($"[Registry] BattlePassRewards loaded: { BattlePassRewardsList.Count } rows");
    }

    static void Load_CombatPowerWeights()
    {
        string jsonText = null;
        // 1) Resources 우선
        if (!string.IsNullOrEmpty(KEY_PREFIX))
        {
            var ta = Resources.Load<TextAsset>(null);
            if (ta != null) jsonText = ta.text;
        }
#if UNITY_EDITOR
        // 2) 에디터 폴백: 파일 직접 읽기
        if (string.IsNullOrEmpty(jsonText))
        {
            var p = System.IO.Path.Combine(JSON_ABS, "CombatPowerWeights.json");
            if (System.IO.File.Exists(p))
                jsonText = System.IO.File.ReadAllText(p, new UTF8Encoding(false));
        }
#endif

        if (string.IsNullOrEmpty(jsonText))
        {
            Debug.LogError($"[Registry] JSON not found: {(KEY_PREFIX==null? JSON_ABS : ("Resources/"+KEY_PREFIX))}/CombatPowerWeights.json");
            return;
        }

        var rows = MiniJsonLite.ParseArray(jsonText);
        if (rows == null) { Debug.LogError("[Registry] JSON parse failed"); return; }
        var list = CombatPowerWeightsList; list.Clear();
        foreach (var map in rows)
        {
            var d = new CombatPowerWeightsRow();
            d.statType = ParseStatType(GetString(map, "statType", ""));
            d.Desc = GetString(map, "Desc", "");
            d.Weight = GetFloat(map, "Weight", 0f);
            list.Add(d);
        }
        Debug.Log($"[Registry] CombatPowerWeights loaded: { CombatPowerWeightsList.Count } rows");
    }

    static void Load_Costumes()
    {
        string jsonText = null;
        // 1) Resources 우선
        if (!string.IsNullOrEmpty(KEY_PREFIX))
        {
            var ta = Resources.Load<TextAsset>(null);
            if (ta != null) jsonText = ta.text;
        }
#if UNITY_EDITOR
        // 2) 에디터 폴백: 파일 직접 읽기
        if (string.IsNullOrEmpty(jsonText))
        {
            var p = System.IO.Path.Combine(JSON_ABS, "Costumes.json");
            if (System.IO.File.Exists(p))
                jsonText = System.IO.File.ReadAllText(p, new UTF8Encoding(false));
        }
#endif

        if (string.IsNullOrEmpty(jsonText))
        {
            Debug.LogError($"[Registry] JSON not found: {(KEY_PREFIX==null? JSON_ABS : ("Resources/"+KEY_PREFIX))}/Costumes.json");
            return;
        }

        var rows = MiniJsonLite.ParseArray(jsonText);
        if (rows == null) { Debug.LogError("[Registry] JSON parse failed"); return; }
        var list = CostumesList; list.Clear();
        foreach (var map in rows)
        {
            var d = new CostumesRow();
            d.ID = GetInt(map, "ID", 0);
            d.Name = GetString(map, "Name", "");
            d.Desc = GetString(map, "Desc", "");
            d.GetWay = GetString(map, "GetWay", "");
            d.Icon = GetString(map, "Icon", "");
            d.StatType1 = GetString(map, "StatType1", "");
            d.StatValue1 = GetFloat(map, "StatValue1", 0f);
            d.StatType2 = GetString(map, "StatType2", "");
            d.StatValue2 = GetFloat(map, "StatValue2", 0f);
            d.StatType3 = GetString(map, "StatType3", "");
            d.StatValue3 = GetFloat(map, "StatValue3", 0f);
            list.Add(d);
        }
        Debug.Log($"[Registry] Costumes loaded: { CostumesList.Count } rows");
    }

    static void Load_DiamondDungeon()
    {
        string jsonText = null;
        // 1) Resources 우선
        if (!string.IsNullOrEmpty(KEY_PREFIX))
        {
            var ta = Resources.Load<TextAsset>(null);
            if (ta != null) jsonText = ta.text;
        }
#if UNITY_EDITOR
        // 2) 에디터 폴백: 파일 직접 읽기
        if (string.IsNullOrEmpty(jsonText))
        {
            var p = System.IO.Path.Combine(JSON_ABS, "DiamondDungeon.json");
            if (System.IO.File.Exists(p))
                jsonText = System.IO.File.ReadAllText(p, new UTF8Encoding(false));
        }
#endif

        if (string.IsNullOrEmpty(jsonText))
        {
            Debug.LogError($"[Registry] JSON not found: {(KEY_PREFIX==null? JSON_ABS : ("Resources/"+KEY_PREFIX))}/DiamondDungeon.json");
            return;
        }

        var rows = MiniJsonLite.ParseArray(jsonText);
        if (rows == null) { Debug.LogError("[Registry] JSON parse failed"); return; }
        var list = DiamondDungeonList; list.Clear();
        foreach (var map in rows)
        {
            var d = new DiamondDungeonRow();
            d.ID = GetInt(map, "ID", 0);
            d.Name = GetString(map, "Name", "");
            d.Type = GetString(map, "Type", "");
            d.Tier = GetInt(map, "Tier", 0);
            d.Monster = GetString(map, "Monster", "");
            d.HPMultiplyer = GetString(map, "HPMultiplyer", "");
            d.RewardItem = GetInt(map, "RewardItem", 0);
            d.RewardAmount = GetInt(map, "RewardAmount", 0);
            d.Time = GetInt(map, "Time", 0);
            d.EnteranceItem = GetInt(map, "EnteranceItem", 0);
            d.EntranceCost = GetInt(map, "EntranceCost", 0);
            list.Add(d);
        }
        Debug.Log($"[Registry] DiamondDungeon loaded: { DiamondDungeonList.Count } rows");
    }

    static void Load_Fames()
    {
        string jsonText = null;
        // 1) Resources 우선
        if (!string.IsNullOrEmpty(KEY_PREFIX))
        {
            var ta = Resources.Load<TextAsset>(null);
            if (ta != null) jsonText = ta.text;
        }
#if UNITY_EDITOR
        // 2) 에디터 폴백: 파일 직접 읽기
        if (string.IsNullOrEmpty(jsonText))
        {
            var p = System.IO.Path.Combine(JSON_ABS, "Fames.json");
            if (System.IO.File.Exists(p))
                jsonText = System.IO.File.ReadAllText(p, new UTF8Encoding(false));
        }
#endif

        if (string.IsNullOrEmpty(jsonText))
        {
            Debug.LogError($"[Registry] JSON not found: {(KEY_PREFIX==null? JSON_ABS : ("Resources/"+KEY_PREFIX))}/Fames.json");
            return;
        }

        var rows = MiniJsonLite.ParseArray(jsonText);
        if (rows == null) { Debug.LogError("[Registry] JSON parse failed"); return; }
        var list = FamesList; list.Clear();
        foreach (var map in rows)
        {
            var d = new FamesRow();
            d.Level = GetInt(map, "Level", 0);
            d.autoDamagePercent = GetFloat(map, "autoDamagePercent", 0f);
            d.autoDamagePercentFame = GetInt(map, "autoDamagePercentFame", 0);
            d.autoAttackSpeedPercent = GetFloat(map, "autoAttackSpeedPercent", 0f);
            d.autoAttackSpeedPercentFame = GetInt(map, "autoAttackSpeedPercentFame", 0);
            d.totalGoldPercent = GetFloat(map, "totalGoldPercent", 0f);
            d.totalGoldPercentFame = GetInt(map, "totalGoldPercentFame", 0);
            d.normalGoldPercent = GetFloat(map, "normalGoldPercent", 0f);
            d.normalGoldPercentFame = GetInt(map, "normalGoldPercentFame", 0);
            d.bossGoldPercent = GetFloat(map, "bossGoldPercent", 0f);
            d.bossGoldPercentFame = GetInt(map, "bossGoldPercentFame", 0);
            d.normalMonsterDamagePercent = GetFloat(map, "normalMonsterDamagePercent", 0f);
            d.normalMonsterDamagePercentFame = GetInt(map, "normalMonsterDamagePercentFame", 0);
            d.bossMonsterDamagePercent = GetFloat(map, "bossMonsterDamagePercent", 0f);
            d.bossMonsterDamagePercentFame = GetInt(map, "bossMonsterDamagePercentFame", 0);
            list.Add(d);
        }
        Debug.Log($"[Registry] Fames loaded: { FamesList.Count } rows");
    }

    static void Load_GoldDungeon()
    {
        string jsonText = null;
        // 1) Resources 우선
        if (!string.IsNullOrEmpty(KEY_PREFIX))
        {
            var ta = Resources.Load<TextAsset>(null);
            if (ta != null) jsonText = ta.text;
        }
#if UNITY_EDITOR
        // 2) 에디터 폴백: 파일 직접 읽기
        if (string.IsNullOrEmpty(jsonText))
        {
            var p = System.IO.Path.Combine(JSON_ABS, "GoldDungeon.json");
            if (System.IO.File.Exists(p))
                jsonText = System.IO.File.ReadAllText(p, new UTF8Encoding(false));
        }
#endif

        if (string.IsNullOrEmpty(jsonText))
        {
            Debug.LogError($"[Registry] JSON not found: {(KEY_PREFIX==null? JSON_ABS : ("Resources/"+KEY_PREFIX))}/GoldDungeon.json");
            return;
        }

        var rows = MiniJsonLite.ParseArray(jsonText);
        if (rows == null) { Debug.LogError("[Registry] JSON parse failed"); return; }
        var list = GoldDungeonList; list.Clear();
        foreach (var map in rows)
        {
            var d = new GoldDungeonRow();
            d.ID = GetInt(map, "ID", 0);
            d.Name = GetString(map, "Name", "");
            d.Type = GetString(map, "Type", "");
            d.Tier = GetInt(map, "Tier", 0);
            d.Monster = GetInt(map, "Monster", 0);
            d.HPMultiplyer = GetInt(map, "HPMultiplyer", 0);
            d.RewardItem = GetInt(map, "RewardItem", 0);
            d.RewardAmount = GetInt(map, "RewardAmount", 0);
            d.Time = GetInt(map, "Time", 0);
            d.EnteranceItem = GetInt(map, "EnteranceItem", 0);
            d.EntranceCost = GetInt(map, "EntranceCost", 0);
            list.Add(d);
        }
        Debug.Log($"[Registry] GoldDungeon loaded: { GoldDungeonList.Count } rows");
    }

    static void Load_GuideQuests()
    {
        string jsonText = null;
        // 1) Resources 우선
        if (!string.IsNullOrEmpty(KEY_PREFIX))
        {
            var ta = Resources.Load<TextAsset>(null);
            if (ta != null) jsonText = ta.text;
        }
#if UNITY_EDITOR
        // 2) 에디터 폴백: 파일 직접 읽기
        if (string.IsNullOrEmpty(jsonText))
        {
            var p = System.IO.Path.Combine(JSON_ABS, "GuideQuests.json");
            if (System.IO.File.Exists(p))
                jsonText = System.IO.File.ReadAllText(p, new UTF8Encoding(false));
        }
#endif

        if (string.IsNullOrEmpty(jsonText))
        {
            Debug.LogError($"[Registry] JSON not found: {(KEY_PREFIX==null? JSON_ABS : ("Resources/"+KEY_PREFIX))}/GuideQuests.json");
            return;
        }

        var rows = MiniJsonLite.ParseArray(jsonText);
        if (rows == null) { Debug.LogError("[Registry] JSON parse failed"); return; }
        var list = GuideQuestsList; list.Clear();
        foreach (var map in rows)
        {
            var d = new GuideQuestsRow();
            d.ID = GetInt(map, "ID", 0);
            d.Name = GetString(map, "Name", "");
            d.QuestType = GetString(map, "QuestType", "");
            d.CompleteCount = GetInt(map, "CompleteCount", 0);
            d.RewardItemType = GetString(map, "RewardItemType", "");
            d.RewardItemID = GetInt(map, "RewardItemID", 0);
            d.RewardAmount = GetInt(map, "RewardAmount", 0);
            list.Add(d);
        }
        Debug.Log($"[Registry] GuideQuests loaded: { GuideQuestsList.Count } rows");
    }

    static void Load_ItemDrops()
    {
        string jsonText = null;
        // 1) Resources 우선
        if (!string.IsNullOrEmpty(KEY_PREFIX))
        {
            var ta = Resources.Load<TextAsset>(null);
            if (ta != null) jsonText = ta.text;
        }
#if UNITY_EDITOR
        // 2) 에디터 폴백: 파일 직접 읽기
        if (string.IsNullOrEmpty(jsonText))
        {
            var p = System.IO.Path.Combine(JSON_ABS, "ItemDrops.json");
            if (System.IO.File.Exists(p))
                jsonText = System.IO.File.ReadAllText(p, new UTF8Encoding(false));
        }
#endif

        if (string.IsNullOrEmpty(jsonText))
        {
            Debug.LogError($"[Registry] JSON not found: {(KEY_PREFIX==null? JSON_ABS : ("Resources/"+KEY_PREFIX))}/ItemDrops.json");
            return;
        }

        var rows = MiniJsonLite.ParseArray(jsonText);
        if (rows == null) { Debug.LogError("[Registry] JSON parse failed"); return; }
        var list = ItemDropsList; list.Clear();
        foreach (var map in rows)
        {
            var d = new ItemDropsRow();
            d.ID = GetInt(map, "ID", 0);
            d.DropItem1 = GetInt(map, "DropItem1", 0);
            d.Probability1 = GetFloat(map, "Probability1", 0f);
            d.DropItem2 = GetInt(map, "DropItem2", 0);
            d.Probability2 = GetFloat(map, "Probability2", 0f);
            d.DropItem3 = GetInt(map, "DropItem3", 0);
            d.Probability3 = GetFloat(map, "Probability3", 0f);
            d.DropItem4 = GetInt(map, "DropItem4", 0);
            d.Probability4 = GetFloat(map, "Probability4", 0f);
            d.DropItem5 = GetInt(map, "DropItem5", 0);
            d.Probability5 = GetFloat(map, "Probability5", 0f);
            d.Validation = GetInt(map, "Validation", 0);
            list.Add(d);
        }
        Debug.Log($"[Registry] ItemDrops loaded: { ItemDropsList.Count } rows");
    }

    static void Load_Items_Etc()
    {
        string jsonText = null;
        // 1) Resources 우선
        if (!string.IsNullOrEmpty(KEY_PREFIX))
        {
            var ta = Resources.Load<TextAsset>(null);
            if (ta != null) jsonText = ta.text;
        }
#if UNITY_EDITOR
        // 2) 에디터 폴백: 파일 직접 읽기
        if (string.IsNullOrEmpty(jsonText))
        {
            var p = System.IO.Path.Combine(JSON_ABS, "Items_Etc.json");
            if (System.IO.File.Exists(p))
                jsonText = System.IO.File.ReadAllText(p, new UTF8Encoding(false));
        }
#endif

        if (string.IsNullOrEmpty(jsonText))
        {
            Debug.LogError($"[Registry] JSON not found: {(KEY_PREFIX==null? JSON_ABS : ("Resources/"+KEY_PREFIX))}/Items_Etc.json");
            return;
        }

        var rows = MiniJsonLite.ParseArray(jsonText);
        if (rows == null) { Debug.LogError("[Registry] JSON parse failed"); return; }
        var list = Items_EtcList; list.Clear();
        foreach (var map in rows)
        {
            var d = new Items_EtcRow();
            d.ID = GetInt(map, "ID", 0);
            d.Name = GetString(map, "Name", "");
            d.Desc = GetString(map, "Desc", "");
            d.Type = GetString(map, "Type", "");
            d.Icon = GetString(map, "Icon", "");
            list.Add(d);
        }
        Debug.Log($"[Registry] Items_Etc loaded: { Items_EtcList.Count } rows");
    }

    static void Load_Monsters()
    {
        string jsonText = null;
        // 1) Resources 우선
        if (!string.IsNullOrEmpty(KEY_PREFIX))
        {
            var ta = Resources.Load<TextAsset>(null);
            if (ta != null) jsonText = ta.text;
        }
#if UNITY_EDITOR
        // 2) 에디터 폴백: 파일 직접 읽기
        if (string.IsNullOrEmpty(jsonText))
        {
            var p = System.IO.Path.Combine(JSON_ABS, "Monsters.json");
            if (System.IO.File.Exists(p))
                jsonText = System.IO.File.ReadAllText(p, new UTF8Encoding(false));
        }
#endif

        if (string.IsNullOrEmpty(jsonText))
        {
            Debug.LogError($"[Registry] JSON not found: {(KEY_PREFIX==null? JSON_ABS : ("Resources/"+KEY_PREFIX))}/Monsters.json");
            return;
        }

        var rows = MiniJsonLite.ParseArray(jsonText);
        if (rows == null) { Debug.LogError("[Registry] JSON parse failed"); return; }
        var list = MonstersList; list.Clear();
        foreach (var map in rows)
        {
            var d = new MonstersRow();
            d.ID = GetInt(map, "ID", 0);
            d.Name = GetString(map, "Name", "");
            d.HP = GetInt(map, "HP", 0);
            d.Image = GetString(map, "Image", "");
            list.Add(d);
        }
        Debug.Log($"[Registry] Monsters loaded: { MonstersList.Count } rows");
    }

    static void Load_Reborn()
    {
        string jsonText = null;
        // 1) Resources 우선
        if (!string.IsNullOrEmpty(KEY_PREFIX))
        {
            var ta = Resources.Load<TextAsset>(null);
            if (ta != null) jsonText = ta.text;
        }
#if UNITY_EDITOR
        // 2) 에디터 폴백: 파일 직접 읽기
        if (string.IsNullOrEmpty(jsonText))
        {
            var p = System.IO.Path.Combine(JSON_ABS, "Reborn.json");
            if (System.IO.File.Exists(p))
                jsonText = System.IO.File.ReadAllText(p, new UTF8Encoding(false));
        }
#endif

        if (string.IsNullOrEmpty(jsonText))
        {
            Debug.LogError($"[Registry] JSON not found: {(KEY_PREFIX==null? JSON_ABS : ("Resources/"+KEY_PREFIX))}/Reborn.json");
            return;
        }

        var rows = MiniJsonLite.ParseArray(jsonText);
        if (rows == null) { Debug.LogError("[Registry] JSON parse failed"); return; }
        var list = RebornList; list.Clear();
        foreach (var map in rows)
        {
            var d = new RebornRow();
            d.ID = GetInt(map, "ID", 0);
            d.Stage = GetInt(map, "Stage", 0);
            d.Treasure = GetInt(map, "Treasure", 0);
            d.Fame = GetInt(map, "Fame", 0);
            d.RebornExp = GetInt(map, "RebornExp", 0);
            list.Add(d);
        }
        Debug.Log($"[Registry] Reborn loaded: { RebornList.Count } rows");
    }

    static void Load_RebornLevel()
    {
        string jsonText = null;
        // 1) Resources 우선
        if (!string.IsNullOrEmpty(KEY_PREFIX))
        {
            var ta = Resources.Load<TextAsset>(null);
            if (ta != null) jsonText = ta.text;
        }
#if UNITY_EDITOR
        // 2) 에디터 폴백: 파일 직접 읽기
        if (string.IsNullOrEmpty(jsonText))
        {
            var p = System.IO.Path.Combine(JSON_ABS, "RebornLevel.json");
            if (System.IO.File.Exists(p))
                jsonText = System.IO.File.ReadAllText(p, new UTF8Encoding(false));
        }
#endif

        if (string.IsNullOrEmpty(jsonText))
        {
            Debug.LogError($"[Registry] JSON not found: {(KEY_PREFIX==null? JSON_ABS : ("Resources/"+KEY_PREFIX))}/RebornLevel.json");
            return;
        }

        var rows = MiniJsonLite.ParseArray(jsonText);
        if (rows == null) { Debug.LogError("[Registry] JSON parse failed"); return; }
        var list = RebornLevelList; list.Clear();
        foreach (var map in rows)
        {
            var d = new RebornLevelRow();
            d.ID = GetInt(map, "ID", 0);
            d.Level = GetInt(map, "Level", 0);
            d.RebornExp = GetInt(map, "RebornExp", 0);
            d.rebornTreasure = GetFloat(map, "rebornTreasure", 0f);
            d.rebornFame = GetFloat(map, "rebornFame", 0f);
            list.Add(d);
        }
        Debug.Log($"[Registry] RebornLevel loaded: { RebornLevelList.Count } rows");
    }

    static void Load_ShopCurrency()
    {
        string jsonText = null;
        // 1) Resources 우선
        if (!string.IsNullOrEmpty(KEY_PREFIX))
        {
            var ta = Resources.Load<TextAsset>(null);
            if (ta != null) jsonText = ta.text;
        }
#if UNITY_EDITOR
        // 2) 에디터 폴백: 파일 직접 읽기
        if (string.IsNullOrEmpty(jsonText))
        {
            var p = System.IO.Path.Combine(JSON_ABS, "ShopCurrency.json");
            if (System.IO.File.Exists(p))
                jsonText = System.IO.File.ReadAllText(p, new UTF8Encoding(false));
        }
#endif

        if (string.IsNullOrEmpty(jsonText))
        {
            Debug.LogError($"[Registry] JSON not found: {(KEY_PREFIX==null? JSON_ABS : ("Resources/"+KEY_PREFIX))}/ShopCurrency.json");
            return;
        }

        var rows = MiniJsonLite.ParseArray(jsonText);
        if (rows == null) { Debug.LogError("[Registry] JSON parse failed"); return; }
        var list = ShopCurrencyList; list.Clear();
        foreach (var map in rows)
        {
            var d = new ShopCurrencyRow();
            d.ID = GetInt(map, "ID", 0);
            d.Type = GetString(map, "Type", "");
            d.ItemID = GetInt(map, "ItemID", 0);
            d.ItemAmount = GetBigNum(map, "ItemAmount");
            d.PriceType = GetString(map, "PriceType", "");
            d.Price = GetInt(map, "Price", 0);
            d.Icon = GetString(map, "Icon", "");
            list.Add(d);
        }
        Debug.Log($"[Registry] ShopCurrency loaded: { ShopCurrencyList.Count } rows");
    }

    static void Load_ShopPackage()
    {
        string jsonText = null;
        // 1) Resources 우선
        if (!string.IsNullOrEmpty(KEY_PREFIX))
        {
            var ta = Resources.Load<TextAsset>(null);
            if (ta != null) jsonText = ta.text;
        }
#if UNITY_EDITOR
        // 2) 에디터 폴백: 파일 직접 읽기
        if (string.IsNullOrEmpty(jsonText))
        {
            var p = System.IO.Path.Combine(JSON_ABS, "ShopPackage.json");
            if (System.IO.File.Exists(p))
                jsonText = System.IO.File.ReadAllText(p, new UTF8Encoding(false));
        }
#endif

        if (string.IsNullOrEmpty(jsonText))
        {
            Debug.LogError($"[Registry] JSON not found: {(KEY_PREFIX==null? JSON_ABS : ("Resources/"+KEY_PREFIX))}/ShopPackage.json");
            return;
        }

        var rows = MiniJsonLite.ParseArray(jsonText);
        if (rows == null) { Debug.LogError("[Registry] JSON parse failed"); return; }
        var list = ShopPackageList; list.Clear();
        foreach (var map in rows)
        {
            var d = new ShopPackageRow();
            d.ID = GetInt(map, "ID", 0);
            d.ItemID1 = GetInt(map, "ItemID1", 0);
            d.ItemAmount1 = GetInt(map, "ItemAmount1", 0);
            d.ItemID2 = GetInt(map, "ItemID2", 0);
            d.ItemAmount2 = GetInt(map, "ItemAmount2", 0);
            d.ItemID3 = GetString(map, "ItemID3", "");
            d.ItemAmount3 = GetString(map, "ItemAmount3", "");
            d.ItemID4 = GetString(map, "ItemID4", "");
            d.ItemAmount4 = GetString(map, "ItemAmount4", "");
            d.ItemID5 = GetString(map, "ItemID5", "");
            d.ItemAmount5 = GetString(map, "ItemAmount5", "");
            d.Price = GetInt(map, "Price", 0);
            list.Add(d);
        }
        Debug.Log($"[Registry] ShopPackage loaded: { ShopPackageList.Count } rows");
    }

    static void Load_Skills()
    {
        string jsonText = null;
        // 1) Resources 우선
        if (!string.IsNullOrEmpty(KEY_PREFIX))
        {
            var ta = Resources.Load<TextAsset>(null);
            if (ta != null) jsonText = ta.text;
        }
#if UNITY_EDITOR
        // 2) 에디터 폴백: 파일 직접 읽기
        if (string.IsNullOrEmpty(jsonText))
        {
            var p = System.IO.Path.Combine(JSON_ABS, "Skills.json");
            if (System.IO.File.Exists(p))
                jsonText = System.IO.File.ReadAllText(p, new UTF8Encoding(false));
        }
#endif

        if (string.IsNullOrEmpty(jsonText))
        {
            Debug.LogError($"[Registry] JSON not found: {(KEY_PREFIX==null? JSON_ABS : ("Resources/"+KEY_PREFIX))}/Skills.json");
            return;
        }

        var rows = MiniJsonLite.ParseArray(jsonText);
        if (rows == null) { Debug.LogError("[Registry] JSON parse failed"); return; }
        var list = SkillsList; list.Clear();
        foreach (var map in rows)
        {
            var d = new SkillsRow();
            d.ID = GetInt(map, "ID", 0);
            d.Name = GetString(map, "Name", "");
            d.Desc = GetString(map, "Desc", "");
            d.Icon = GetString(map, "Icon", "");
            d.Type = GetString(map, "Type", "");
            d.Value = GetInt(map, "Value", 0);
            d.valueGrowth = GetFloat(map, "valueGrowth", 0f);
            d.coolTime = GetInt(map, "coolTime", 0);
            d.duration = GetInt(map, "duration", 0);
            list.Add(d);
        }
        Debug.Log($"[Registry] Skills loaded: { SkillsList.Count } rows");
    }

    static void Load_Stages()
    {
        string jsonText = null;
        // 1) Resources 우선
        if (!string.IsNullOrEmpty(KEY_PREFIX))
        {
            var ta = Resources.Load<TextAsset>(null);
            if (ta != null) jsonText = ta.text;
        }
#if UNITY_EDITOR
        // 2) 에디터 폴백: 파일 직접 읽기
        if (string.IsNullOrEmpty(jsonText))
        {
            var p = System.IO.Path.Combine(JSON_ABS, "Stages.json");
            if (System.IO.File.Exists(p))
                jsonText = System.IO.File.ReadAllText(p, new UTF8Encoding(false));
        }
#endif

        if (string.IsNullOrEmpty(jsonText))
        {
            Debug.LogError($"[Registry] JSON not found: {(KEY_PREFIX==null? JSON_ABS : ("Resources/"+KEY_PREFIX))}/Stages.json");
            return;
        }

        var rows = MiniJsonLite.ParseArray(jsonText);
        if (rows == null) { Debug.LogError("[Registry] JSON parse failed"); return; }
        var list = StagesList; list.Clear();
        foreach (var map in rows)
        {
            var d = new StagesRow();
            d.ID = GetInt(map, "ID", 0);
            d.Name = GetString(map, "Name", "");
            d.Gold = GetBigNum(map, "Gold");
            d.monsterID = GetInt(map, "monsterID", 0);
            d.isBoss = GetInt(map, "isBoss", 0);
            d.bossTime = GetInt(map, "bossTime", 0);
            d.HPMultiplyer = GetBigNum(map, "HPMultiplyer");
            d.ItemDrops = GetInt(map, "ItemDrops", 0);
            d.Background = GetString(map, "Background", "");
            list.Add(d);
        }
        Debug.Log($"[Registry] Stages loaded: { StagesList.Count } rows");
    }

    static void Load_StatGrowthGolds()
    {
        string jsonText = null;
        // 1) Resources 우선
        if (!string.IsNullOrEmpty(KEY_PREFIX))
        {
            var ta = Resources.Load<TextAsset>(null);
            if (ta != null) jsonText = ta.text;
        }
#if UNITY_EDITOR
        // 2) 에디터 폴백: 파일 직접 읽기
        if (string.IsNullOrEmpty(jsonText))
        {
            var p = System.IO.Path.Combine(JSON_ABS, "StatGrowthGolds.json");
            if (System.IO.File.Exists(p))
                jsonText = System.IO.File.ReadAllText(p, new UTF8Encoding(false));
        }
#endif

        if (string.IsNullOrEmpty(jsonText))
        {
            Debug.LogError($"[Registry] JSON not found: {(KEY_PREFIX==null? JSON_ABS : ("Resources/"+KEY_PREFIX))}/StatGrowthGolds.json");
            return;
        }

        var rows = MiniJsonLite.ParseArray(jsonText);
        if (rows == null) { Debug.LogError("[Registry] JSON parse failed"); return; }
        var list = StatGrowthGoldsList; list.Clear();
        foreach (var map in rows)
        {
            var d = new StatGrowthGoldsRow();
            d.Level = GetInt(map, "Level", 0);
            d.autoAttackDamage = GetInt(map, "autoAttackDamage", 0);
            d.autoAttackDamageGold = GetFloat(map, "autoAttackDamageGold", 0f);
            d.autoAttackSpeed = GetFloat(map, "autoAttackSpeed", 0f);
            d.autoAttackSpeedGold = GetInt(map, "autoAttackSpeedGold", 0);
            d.criticalProbability = GetFloat(map, "criticalProbability", 0f);
            d.criticalProbabilityGold = GetFloat(map, "criticalProbabilityGold", 0f);
            d.criticalDamage = GetFloat(map, "criticalDamage", 0f);
            d.criticalDamageGold = GetBigNum(map, "criticalDamageGold");
            list.Add(d);
        }
        Debug.Log($"[Registry] StatGrowthGolds loaded: { StatGrowthGoldsList.Count } rows");
    }

    static void Load_StatRerollGemDungeon()
    {
        string jsonText = null;
        // 1) Resources 우선
        if (!string.IsNullOrEmpty(KEY_PREFIX))
        {
            var ta = Resources.Load<TextAsset>(null);
            if (ta != null) jsonText = ta.text;
        }
#if UNITY_EDITOR
        // 2) 에디터 폴백: 파일 직접 읽기
        if (string.IsNullOrEmpty(jsonText))
        {
            var p = System.IO.Path.Combine(JSON_ABS, "StatRerollGemDungeon.json");
            if (System.IO.File.Exists(p))
                jsonText = System.IO.File.ReadAllText(p, new UTF8Encoding(false));
        }
#endif

        if (string.IsNullOrEmpty(jsonText))
        {
            Debug.LogError($"[Registry] JSON not found: {(KEY_PREFIX==null? JSON_ABS : ("Resources/"+KEY_PREFIX))}/StatRerollGemDungeon.json");
            return;
        }

        var rows = MiniJsonLite.ParseArray(jsonText);
        if (rows == null) { Debug.LogError("[Registry] JSON parse failed"); return; }
        var list = StatRerollGemDungeonList; list.Clear();
        foreach (var map in rows)
        {
            var d = new StatRerollGemDungeonRow();
            d.ID = GetInt(map, "ID", 0);
            d.Name = GetString(map, "Name", "");
            d.Type = GetString(map, "Type", "");
            d.Tier = GetInt(map, "Tier", 0);
            d.Monster = GetInt(map, "Monster", 0);
            d.HPMultiplyer = GetInt(map, "HPMultiplyer", 0);
            d.RewardItem = GetInt(map, "RewardItem", 0);
            d.RewardAmount = GetInt(map, "RewardAmount", 0);
            d.Time = GetInt(map, "Time", 0);
            d.EnteranceItem = GetInt(map, "EnteranceItem", 0);
            d.EntranceCost = GetInt(map, "EntranceCost", 0);
            list.Add(d);
        }
        Debug.Log($"[Registry] StatRerollGemDungeon loaded: { StatRerollGemDungeonList.Count } rows");
    }

    static void Load_Stats()
    {
        string jsonText = null;
        // 1) Resources 우선
        if (!string.IsNullOrEmpty(KEY_PREFIX))
        {
            var ta = Resources.Load<TextAsset>(null);
            if (ta != null) jsonText = ta.text;
        }
#if UNITY_EDITOR
        // 2) 에디터 폴백: 파일 직접 읽기
        if (string.IsNullOrEmpty(jsonText))
        {
            var p = System.IO.Path.Combine(JSON_ABS, "Stats.json");
            if (System.IO.File.Exists(p))
                jsonText = System.IO.File.ReadAllText(p, new UTF8Encoding(false));
        }
#endif

        if (string.IsNullOrEmpty(jsonText))
        {
            Debug.LogError($"[Registry] JSON not found: {(KEY_PREFIX==null? JSON_ABS : ("Resources/"+KEY_PREFIX))}/Stats.json");
            return;
        }

        var rows = MiniJsonLite.ParseArray(jsonText);
        if (rows == null) { Debug.LogError("[Registry] JSON parse failed"); return; }
        var list = StatsList; list.Clear();
        foreach (var map in rows)
        {
            var d = new StatsRow();
            d.ID = GetInt(map, "ID", 0);
            d.statType = ParseStatType(GetString(map, "statType", ""));
            d.Desc = GetString(map, "Desc", "");
            d.Order = GetInt(map, "Order", 0);
            d.Icon = GetString(map, "Icon", "");
            d.Funtest = GetString(map, "Funtest", "");
            list.Add(d);
        }
        Debug.Log($"[Registry] Stats loaded: { StatsList.Count } rows");
    }

    static void Load_WeaponGacha()
    {
        string jsonText = null;
        // 1) Resources 우선
        if (!string.IsNullOrEmpty(KEY_PREFIX))
        {
            var ta = Resources.Load<TextAsset>(null);
            if (ta != null) jsonText = ta.text;
        }
#if UNITY_EDITOR
        // 2) 에디터 폴백: 파일 직접 읽기
        if (string.IsNullOrEmpty(jsonText))
        {
            var p = System.IO.Path.Combine(JSON_ABS, "WeaponGacha.json");
            if (System.IO.File.Exists(p))
                jsonText = System.IO.File.ReadAllText(p, new UTF8Encoding(false));
        }
#endif

        if (string.IsNullOrEmpty(jsonText))
        {
            Debug.LogError($"[Registry] JSON not found: {(KEY_PREFIX==null? JSON_ABS : ("Resources/"+KEY_PREFIX))}/WeaponGacha.json");
            return;
        }

        var rows = MiniJsonLite.ParseArray(jsonText);
        if (rows == null) { Debug.LogError("[Registry] JSON parse failed"); return; }
        var list = WeaponGachaList; list.Clear();
        foreach (var map in rows)
        {
            var d = new WeaponGachaRow();
            d.ID = GetInt(map, "ID", 0);
            d.GachaCostItem = GetInt(map, "GachaCostItem", 0);
            d.GachaCost = GetInt(map, "GachaCost", 0);
            d.GachaLevel = GetInt(map, "GachaLevel", 0);
            d.RequireExp = GetInt(map, "RequireExp", 0);
            d.GachaExpPerTry = GetInt(map, "GachaExpPerTry", 0);
            d.CommonProb = GetFloat(map, "CommonProb", 0f);
            d.UncommonProb = GetFloat(map, "UncommonProb", 0f);
            d.RareProb = GetFloat(map, "RareProb", 0f);
            d.LegendProb = GetFloat(map, "LegendProb", 0f);
            d.MythicProb = GetFloat(map, "MythicProb", 0f);
            d.Validation = GetInt(map, "Validation", 0);
            list.Add(d);
        }
        Debug.Log($"[Registry] WeaponGacha loaded: { WeaponGachaList.Count } rows");
    }

    static void Load_WeaponOptionLock()
    {
        string jsonText = null;
        // 1) Resources 우선
        if (!string.IsNullOrEmpty(KEY_PREFIX))
        {
            var ta = Resources.Load<TextAsset>(null);
            if (ta != null) jsonText = ta.text;
        }
#if UNITY_EDITOR
        // 2) 에디터 폴백: 파일 직접 읽기
        if (string.IsNullOrEmpty(jsonText))
        {
            var p = System.IO.Path.Combine(JSON_ABS, "WeaponOptionLock.json");
            if (System.IO.File.Exists(p))
                jsonText = System.IO.File.ReadAllText(p, new UTF8Encoding(false));
        }
#endif

        if (string.IsNullOrEmpty(jsonText))
        {
            Debug.LogError($"[Registry] JSON not found: {(KEY_PREFIX==null? JSON_ABS : ("Resources/"+KEY_PREFIX))}/WeaponOptionLock.json");
            return;
        }

        var rows = MiniJsonLite.ParseArray(jsonText);
        if (rows == null) { Debug.LogError("[Registry] JSON parse failed"); return; }
        var list = WeaponOptionLockList; list.Clear();
        foreach (var map in rows)
        {
            var d = new WeaponOptionLockRow();
            d.ID = GetInt(map, "ID", 0);
            d.LockItem = GetInt(map, "LockItem", 0);
            d.LockCount = GetInt(map, "LockCount", 0);
            d.LockItemAmount = GetInt(map, "LockItemAmount", 0);
            list.Add(d);
        }
        Debug.Log($"[Registry] WeaponOptionLock loaded: { WeaponOptionLockList.Count } rows");
    }

    static void Load_WeaponOptions()
    {
        string jsonText = null;
        // 1) Resources 우선
        if (!string.IsNullOrEmpty(KEY_PREFIX))
        {
            var ta = Resources.Load<TextAsset>(null);
            if (ta != null) jsonText = ta.text;
        }
#if UNITY_EDITOR
        // 2) 에디터 폴백: 파일 직접 읽기
        if (string.IsNullOrEmpty(jsonText))
        {
            var p = System.IO.Path.Combine(JSON_ABS, "WeaponOptions.json");
            if (System.IO.File.Exists(p))
                jsonText = System.IO.File.ReadAllText(p, new UTF8Encoding(false));
        }
#endif

        if (string.IsNullOrEmpty(jsonText))
        {
            Debug.LogError($"[Registry] JSON not found: {(KEY_PREFIX==null? JSON_ABS : ("Resources/"+KEY_PREFIX))}/WeaponOptions.json");
            return;
        }

        var rows = MiniJsonLite.ParseArray(jsonText);
        if (rows == null) { Debug.LogError("[Registry] JSON parse failed"); return; }
        var list = WeaponOptionsList; list.Clear();
        foreach (var map in rows)
        {
            var d = new WeaponOptionsRow();
            d.ID = GetInt(map, "ID", 0);
            d.Name = GetString(map, "Name", "");
            d.statType = ParseStatType(GetString(map, "statType", ""));
            d.minCommon = GetFloat(map, "minCommon", 0f);
            d.maxCommon = GetFloat(map, "maxCommon", 0f);
            d.growthCommon = GetFloat(map, "growthCommon", 0f);
            d.minUncommon = GetFloat(map, "minUncommon", 0f);
            d.maxUncommon = GetFloat(map, "maxUncommon", 0f);
            d.growthUncommon = GetFloat(map, "growthUncommon", 0f);
            d.minRare = GetFloat(map, "minRare", 0f);
            d.maxRare = GetFloat(map, "maxRare", 0f);
            d.growthRare = GetFloat(map, "growthRare", 0f);
            d.minLegend = GetFloat(map, "minLegend", 0f);
            d.maxLegend = GetFloat(map, "maxLegend", 0f);
            d.growthLegend = GetFloat(map, "growthLegend", 0f);
            d.minMythic = GetFloat(map, "minMythic", 0f);
            d.maxMythic = GetFloat(map, "maxMythic", 0f);
            d.growthMythic = GetFloat(map, "growthMythic", 0f);
            list.Add(d);
        }
        Debug.Log($"[Registry] WeaponOptions loaded: { WeaponOptionsList.Count } rows");
    }

    static void Load_Weapons()
    {
        string jsonText = null;
        // 1) Resources 우선
        if (!string.IsNullOrEmpty(KEY_PREFIX))
        {
            var ta = Resources.Load<TextAsset>(null);
            if (ta != null) jsonText = ta.text;
        }
#if UNITY_EDITOR
        // 2) 에디터 폴백: 파일 직접 읽기
        if (string.IsNullOrEmpty(jsonText))
        {
            var p = System.IO.Path.Combine(JSON_ABS, "Weapons.json");
            if (System.IO.File.Exists(p))
                jsonText = System.IO.File.ReadAllText(p, new UTF8Encoding(false));
        }
#endif

        if (string.IsNullOrEmpty(jsonText))
        {
            Debug.LogError($"[Registry] JSON not found: {(KEY_PREFIX==null? JSON_ABS : ("Resources/"+KEY_PREFIX))}/Weapons.json");
            return;
        }

        var rows = MiniJsonLite.ParseArray(jsonText);
        if (rows == null) { Debug.LogError("[Registry] JSON parse failed"); return; }
        var list = WeaponsList; list.Clear();
        foreach (var map in rows)
        {
            var d = new WeaponsRow();
            d.ID = GetInt(map, "ID", 0);
            d.Name = GetString(map, "Name", "");
            d.Desc = GetString(map, "Desc", "");
            d.Icon = GetString(map, "Icon", "");
            d.IconRank = GetString(map, "IconRank", "");
            d.Rank = GetInt(map, "Rank", 0);
            d.specialAbilityType = GetString(map, "specialAbilityType", "");
            d.specialAbilityValue = GetFloat(map, "specialAbilityValue", 0f);
            d.autoAttackDamage = GetInt(map, "autoAttackDamage", 0);
            list.Add(d);
        }
        Debug.Log($"[Registry] Weapons loaded: { WeaponsList.Count } rows");
    }

    static void Load_WeaponStatReroll()
    {
        string jsonText = null;
        // 1) Resources 우선
        if (!string.IsNullOrEmpty(KEY_PREFIX))
        {
            var ta = Resources.Load<TextAsset>(null);
            if (ta != null) jsonText = ta.text;
        }
#if UNITY_EDITOR
        // 2) 에디터 폴백: 파일 직접 읽기
        if (string.IsNullOrEmpty(jsonText))
        {
            var p = System.IO.Path.Combine(JSON_ABS, "WeaponStatReroll.json");
            if (System.IO.File.Exists(p))
                jsonText = System.IO.File.ReadAllText(p, new UTF8Encoding(false));
        }
#endif

        if (string.IsNullOrEmpty(jsonText))
        {
            Debug.LogError($"[Registry] JSON not found: {(KEY_PREFIX==null? JSON_ABS : ("Resources/"+KEY_PREFIX))}/WeaponStatReroll.json");
            return;
        }

        var rows = MiniJsonLite.ParseArray(jsonText);
        if (rows == null) { Debug.LogError("[Registry] JSON parse failed"); return; }
        var list = WeaponStatRerollList; list.Clear();
        foreach (var map in rows)
        {
            var d = new WeaponStatRerollRow();
            d.ID = GetInt(map, "ID", 0);
            d.LockCount = GetInt(map, "LockCount", 0);
            d.CommonReroll = GetInt(map, "CommonReroll", 0);
            d.UncommonReroll = GetInt(map, "UncommonReroll", 0);
            d.RareReroll = GetInt(map, "RareReroll", 0);
            d.LegendReroll = GetInt(map, "LegendReroll", 0);
            d.MythicReroll = GetInt(map, "MythicReroll", 0);
            list.Add(d);
        }
        Debug.Log($"[Registry] WeaponStatReroll loaded: { WeaponStatRerollList.Count } rows");
    }

}
