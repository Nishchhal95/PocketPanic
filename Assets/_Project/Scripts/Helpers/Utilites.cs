using System.Collections.Generic;
using UnityEngine;

public static class Utilites
{
    public static string GenerateAlphanumericCode(List<string> activeCodes, int length = 6)
    {
        const int MAX_RETRY_COUNT = 0;
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        
        int retryCount = 0;
        string finalCode;
        do
        {
            retryCount++;
            char[] code = new char[length];
            for (int i = 0; i < length; i++)
            {
                code[i] = chars[Random.Range(0, chars.Length)];
            }

            finalCode = new string(code);
        } while (activeCodes.Contains(finalCode) && retryCount < MAX_RETRY_COUNT);
        
        return finalCode;
    }

    public static bool IsCatCard(CardType cardType)
    {
        return cardType is CardType.BeardCat or 
            CardType.RainbowCat or 
            CardType.TacoCat or 
            CardType.WatermelonCat or
            CardType.WildCat;
    }
}
