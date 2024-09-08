using System;
using System.Collections;
using System.Collections.Generic;

public class CardManager
{
    public static List<Card> cards = new List<Card>();

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
    
    public static void Shuffle()
    {
        cards.Clear();
    
        for (int suit = 1; suit <= 4; suit++)
        {
            for (int rank = 1; rank <= 13; rank++)
            {
                Card card = new Card(suit, rank);
                cards.Add(card);
            }
        }
        
        cards.Add(new Card(CardSuit.None, CardRank.BlackJoker));
        cards.Add(new Card(CardSuit.None, CardRank.RedJoker));

        Queue<Card> cardQueue = new Queue<Card>();
        Random random = new Random();
        for (int i = 0; i < 54; i++)
        {
            int index = random.Next(cards.Count);
            cardQueue.Enqueue(cards[index]);
            cards.RemoveAt(index);
        }

        for (int i = 0; i < 54; i++)
        {
            cards.Add(cardQueue.Dequeue());
        }
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
    
    public static CardType GetCardType(List<Card> cards, out int typeRank)
    {
        typeRank = -1;
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
            if (IsChain(ranks, out typeRank))
                cardType = CardType.Chain;
            else if (ranks[0] == ranks[1] && ranks[1] == ranks[2] && ranks[3] == ranks[4] ||
                     ranks[2] == ranks[3] && ranks[3] == ranks[4] && ranks[0] == ranks[1])
                cardType = CardType.ThreeWithTwo;
        }
        else if (length >= 6)
        {
            if (IsPlane(ranks, out typeRank)) cardType = CardType.Plane;
            else if (IsPlaneWithOne(ranks, out typeRank)) cardType = CardType.PlaneWithOne;
            else if (IsPlaneWithTwo(ranks, out typeRank)) cardType = CardType.PlaneWithTwo;
            else if (IsChain(ranks, out typeRank)) cardType = CardType.Chain;
            else if (IsPairChain(ranks, out typeRank)) cardType = CardType.PairChain;
            else if (IsFourWithTwo(ranks)) cardType = CardType.FourWithTwo;
        }

        return cardType;
    }

    public static bool Compare(Card[] preCards, Card[] curCards)
    {
        var preList = preCards.ToList();
        var curList = curCards.ToList();
        preList.Sort((a, b) => (int)a.cardRank - (int)b.cardRank);
        curList.Sort((a, b) => (int)a.cardRank - (int)b.cardRank);
        var preType = GetCardType(preList,out int preRank);
        var curType = GetCardType(curList, out int curRank);
        if (curType == CardType.JokerBomb) return true;
        else if (curType == CardType.NormalBomb && preType != CardType.NormalBomb)
            return true;
        else if (preType == curType)
        {
            switch (curType)
            {
                case CardType.None:
                    break;
                case CardType.One:
                    return curList[0].cardRank > preList[0].cardRank;
                case CardType.Two:
                    return curList[0].cardRank > preList[0].cardRank;
                case CardType.Three:
                    return curList[0].cardRank > preList[0].cardRank;
                case CardType.ThreeWithOne:
                    return curList[1].cardRank > preList[1].cardRank;
                case CardType.ThreeWithTwo:
                    return curList[2].cardRank > preList[2].cardRank;
                case CardType.Plane:
                    return curRank > preRank;
                case CardType.PlaneWithOne:
                    return curRank > preRank;
                case CardType.PlaneWithTwo:
                    return curRank > preRank;
                case CardType.Chain:
                    return curRank > preRank;
                case CardType.PairChain:
                    return curRank > preRank;
                case CardType.FourWithTwo:
                    return curList[2].cardRank > preList[2].cardRank;
                case CardType.NormalBomb:
                    return curList[0].cardRank > preList[0].cardRank;
            }
        }
        return false;
    }
    
    
    
    
    #region DetectCardType

    private static bool IsPlane(int[] sortRanks, out int typeRank)
    {
        typeRank = -1;
        if (sortRanks.Length % 3 != 0) return false;
        for (int i = 0; i < sortRanks.Length; i+=3)
        {
            if (!(sortRanks[i] == sortRanks[i + 1] && sortRanks[i] == sortRanks[i + 2]))
                return false;
            if (i > 0 && sortRanks[i] - sortRanks[i - 3] != 1) 
                return false;
        }

        typeRank = sortRanks.Last();

        return true;
    }
    private static bool IsPlaneWithOne(int[] sortRanks, out int typeRank)
    {
        typeRank = -1;
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
                typeRank = mainRanks[i];
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
    private static bool IsPlaneWithTwo(int[] sortRanks, out int typeRank)
    {
        typeRank = -1;
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
                typeRank = mainRanks[i];
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
    private static bool IsChain(int[] sortRanks, out int typeRank)
    {
        typeRank = -1;
        if (sortRanks.Contains((int)CardRank._2)) return false;
        for (int i = 1; i < sortRanks.Length; i++)
        {
            if (sortRanks[i] - sortRanks[i - 1] != 1)
                return false;
        }

        return true;
    }
    private static bool IsPairChain(int[] sortRanks, out int typeRank)
    {
        typeRank = -1;
        if (sortRanks.Length % 2 != 0) return false;
        if (sortRanks.Contains((int)CardRank._2)) return false;
        if (sortRanks[0] != sortRanks[1]) return false;
        for (int i = 2; i < sortRanks.Length; i+=2)
        {
            if (!(sortRanks[i] - sortRanks[i - 1] == 1 && sortRanks[i] == sortRanks[i + 1]))
                return false;
        }

        typeRank = sortRanks.Last();
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
    #endregion
}
