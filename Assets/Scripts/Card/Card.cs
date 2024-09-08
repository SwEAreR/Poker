using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardSuit
{
    None,
    Diamond,
    Club,
    Heart,
    Spade
}
    
public enum CardRank
{
    None,
    _3,
    _4,
    _5,
    _6,
    _7,
    _8,
    _9,
    _10,
    _11,
    _12,
    _13,
    _1,
    _2,
    BlackJoker,
    RedJoker
}

public class Card
{
    public CardSuit cardSuit;
    public CardRank cardRank;
    public Card(CardSuit cardSuit, CardRank cardRank)
    {
        this.cardSuit = cardSuit;
        this.cardRank = cardRank;
    }
    public Card(int suit, int rank)
    {
        this.cardSuit = (CardSuit)suit;
        this.cardRank = (CardRank)rank;
    }
    
    public override bool Equals(object? obj)
    {
        Card card = obj as Card;
        if (card == null) return false;
        return cardSuit == card.cardSuit && cardRank == card.cardRank;
    }

    public override int GetHashCode()
    {
        return Tuple.Create(cardSuit, cardRank).GetHashCode();
    }
}
