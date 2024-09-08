using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class CardManager
{
    public static Dictionary<string, Card> cardsDic = new Dictionary<string, Card>();

    public enum CardType
    {
        None,
        One,            //1
        Two,            //2
        Three,          //3
        ThreeWithOne,   //4
        ThreeWithTwo,   //5
        Plane,          //6、9、12、15、18
        PlaneWithOne,   //8、12、16、20
        PlaneWithTwo,   //10、15、20
        Chain,          //5~12 2不能
        PairChain,      //6、8、10、12、14、16、18、20
        FourWithTwo,    //6
        NormalBomb,     //4
        JokerBomb       //2
    }
    
    public static void Init()
    {
        for (int suit = 1; suit <= 4; suit++)
        {
            for (int num = 1; num <= 13; num++)
            {
                Card card = new Card(suit, num);
                string name = ((CardSuit)suit).ToString() + ((CardRank)num).ToString();
                cardsDic.Add(name, card);
            }
        }
        
        cardsDic.Add("BlackJoker", new Card(CardSuit.None, CardRank.BlackJoker));
        cardsDic.Add("RedJoker", new Card(CardSuit.None, CardRank.RedJoker));
    }

    public static string GetName(Card card)
    {
        foreach (string name in cardsDic.Keys)
        {
            if (cardsDic[name].cardSuit == card.cardSuit && cardsDic[name].cardRank == card.cardRank)
                return name;
        }

        return "";
    }

    public static Card GetCard(string name)
    {
        if (cardsDic.TryGetValue(name, out var card))
        {
            return card;
        }
        else
        {
            return null;
        }
    }

    public static List<CardData> CardToData(List<Card> cards)
    {
        List<CardData> datas = new List<CardData>();
        foreach (Card card in cards)
        {
            datas.Add(new CardData
            {
                cardRank = (int)card.cardRank,
                cardSuit = (int)card.cardSuit
            });
        }

        return datas;
    }
    
    public static List<Card> DataToCard(List<CardData> datas)
    {
        List<Card> cards = new List<Card>();
        foreach (CardData data in datas)
        {
            cards.Add(new Card(data.cardSuit, data.cardRank));
        }

        return cards;
    }

    public static CardType GetCardType(List<Card> cards)
    {
        CardType cardType = CardType.None;
        int length = cards.Count;
        int[] ranks = new int[length];
        for (var i = 0; i < length; i++)
        {
            ranks[i] = (int)cards[i].cardRank;
        }
        Array.Sort(ranks);

        if (length == 1)
        {
            cardType = CardType.One;
        }
        else if (length == 2)
        {
            if (ranks[0] == ranks[1])
            {
                cardType = CardType.Two;
            }
            else if (ranks[0] == (int)CardRank.BlackJoker && ranks[1] == (int)CardRank.RedJoker)
                cardType = CardType.JokerBomb;
        }
        else if (length == 3)
        {
            if (ranks[0] == ranks[1] && ranks[1] == ranks[2])
                cardType = CardType.Three;
        }
        else if (length == 4)
        {
            if (ranks[0] == ranks[1] && ranks[1] == ranks[2] && ranks[2] == ranks[3])
                cardType = CardType.NormalBomb;
            else if (ranks[0] == ranks[1] && ranks[1] == ranks[2] || 
                     ranks[1] == ranks[2] && ranks[2] == ranks[3])
                cardType = CardType.ThreeWithOne;
        }
        else if (length == 5)
        {
            if (IsChain(ranks))
                cardType = CardType.Chain;
            else if (ranks[0] == ranks[1] && ranks[1] == ranks[2] && ranks[3] == ranks[4] ||
                     ranks[2] == ranks[3] && ranks[3] == ranks[4] && ranks[0] == ranks[1])
                cardType = CardType.ThreeWithTwo;
        }
        else if (length >= 6)
        {
            if (IsPlane(ranks)) cardType = CardType.Plane;
            else if (IsPlaneWithOne(ranks)) cardType = CardType.PlaneWithOne;
            else if (IsPlaneWithTwo(ranks)) cardType = CardType.PlaneWithTwo;
            else if (IsChain(ranks)) cardType = CardType.Chain;
            else if (IsPairChain(ranks)) cardType = CardType.PairChain;
            else if (IsFourWithTwo(ranks)) cardType = CardType.FourWithTwo;
        }

        return cardType;
    }

    private static bool IsPlane(int[] sortRanks)
    {
        if (sortRanks.Length % 3 != 0) return false;
        for (int i = 0; i < sortRanks.Length; i+=3)
        {
            if (!(sortRanks[i] == sortRanks[i + 1] && sortRanks[i] == sortRanks[i + 2]))
                return false;
            if (i > 0 && sortRanks[i] - sortRanks[i - 3] != 1) 
                return false;
        }

        return true;
    }
    private static bool IsPlaneWithOne(int[] sortRanks)
    {
        if (sortRanks.Length % 4 != 0) return false;
        int planeCount = sortRanks.Length / 4;
        Dictionary<int, int> dict = new Dictionary<int, int>();
        foreach (int rank in sortRanks)
        {
            if (dict.ContainsKey(rank))
            {
                dict[rank]++;
            }
            else
            {
                dict.Add(rank, 1);
            }
        }

        List<int> mainRanks = new List<int>();
        foreach (var pair in dict)
        {
            if (pair.Value == 3)
            {
                mainRanks.Add(pair.Key);
            }
        }
        mainRanks.Sort();
        int sibling = 0;
        int siblingMax = 0;
        if (mainRanks.Count < 2) return false;
        for (int i = 1; i < mainRanks.Count; i++)
        {
            if (mainRanks[i] - mainRanks[i-1] == 1)
            {
                sibling++;
            }
            else
            {
                siblingMax = sibling;
                sibling = 0;
            }
        }

        if (sibling > siblingMax)
        {
            siblingMax = sibling;
            sibling = 0;
        }

        if (planeCount == siblingMax + 1)
            return true;

        return false;
    }
    private static bool IsPlaneWithTwo(int[] sortRanks)
    {
        if (sortRanks.Length % 5 != 0) return false;
        int planeCount = sortRanks.Length / 5;
        Dictionary<int, int> dict = new Dictionary<int, int>();
        foreach (int rank in sortRanks)
        {
            if (dict.ContainsKey(rank))
            {
                dict[rank]++;
            }
            else
            {
                dict.Add(rank, 1);
            }
        }

        int twoCount = 0;
        List<int> mainRanks = new List<int>();
        foreach (var pair in dict)
        {
            if (pair.Value == 3)
            {
                mainRanks.Add(pair.Key);
            }
            else if (pair.Value == 2)
            {
                twoCount++;
            }
            else if (pair.Value == 4)
            {
                twoCount += 2;
            }
        }
        mainRanks.Sort();
        int sibling = 0;
        int siblingMax = 0;
        if (mainRanks.Count < 2) return false;
        for (int i = 1; i < mainRanks.Count; i++)
        {
            if (mainRanks[i] - mainRanks[i-1] == 1)
            {
                sibling++;
            }
            else
            {
                siblingMax = sibling;
                sibling = 0;
            }
        }

        if (sibling > siblingMax)
        {
            siblingMax = sibling;
            sibling = 0;
        }

        if (planeCount == siblingMax + 1 && planeCount == twoCount)
            return true;

        return false;
    }
    private static bool IsChain(int[] sortRanks)
    {
        if (sortRanks.Contains((int)CardRank._2)) return false;
        for (int i = 1; i < sortRanks.Length; i++)
        {
            if (sortRanks[i] - sortRanks[i - 1] != 1)
                return false;
        }

        return true;
    }
    private static bool IsPairChain(int[] sortRanks)
    {
        if (sortRanks.Length % 2 != 0) return false;
        if (sortRanks.Contains((int)CardRank._2)) return false;
        if (sortRanks[0] != sortRanks[1]) return false;
        for (int i = 2; i < sortRanks.Length; i+=2)
        {
            if (!(sortRanks[i] - sortRanks[i - 1] == 1 && sortRanks[i] == sortRanks[i + 1]))
                return false;
        }

        return true;
    }
    private static bool IsFourWithTwo(int[] sortRanks)
    {
        if (sortRanks.Length != 6) return false;
        if (sortRanks[0] == sortRanks[1] && sortRanks[1] == sortRanks[2] && sortRanks[2] == sortRanks[3] &&
            sortRanks[4] == sortRanks[5] ||
            sortRanks[0] == sortRanks[1] && sortRanks[2] == sortRanks[3] && sortRanks[3] == sortRanks[4] &&
            sortRanks[4] == sortRanks[5])
            return true;

        return false;
    }
    
    public static void RemovePlayCards(List<Card> cards)
    {
        for (int i = 0; i < cards.Count; i++)
        {
            for (int j = GameManager.cards.Count - 1; j >= 0; j--)
            {
                if (cards[i].Equals(GameManager.cards[j]))
                {
                    GameManager.cards.RemoveAt(j);
                }
            }
        }
    }
    
    public static void InitCardsPosition()
    {
        Transform cardsParent = GameManager.Myselfobj.transform.Find("ShowCards");
        for (int i = 0; i < cardsParent.childCount; i++)
        {
            var pos = cardsParent.GetChild(i).localPosition;
            cardsParent.GetChild(i).localPosition = new Vector3(pos.x, -50, pos.z);
        }
    }
}
