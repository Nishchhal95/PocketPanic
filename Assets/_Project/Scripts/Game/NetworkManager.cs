using System;
using Photon.Pun;
using UnityEngine;

public class NetworkManager : MonoBehaviourPun
{
    public event Action<int, int> OnGameStarted;
    public event Action<int, int, bool> OnCardDrawn;
    public event Action OnEndTurn;
    public event Action<int> OnSetActorTurn;
    public event Action<int, int> OnDefuseUsed;
    public event Action<int> OnPlayerExploded;
    public event Action<int, int> OnActionCardPlayed;
    public event Action<int> OnShuffleDeck;
    public event Action<int, int> OnRequestFavorCard;
    public event Action<int, int, int> OnResponseFavorCard;
    public event Action<int, int[]> OnAlterTheFutureCardsChanged;
    public event Action<int, int, int> OnStealCardWith2CardCombo;
    
    public void SendStartGame(int seed1, int seed2) => photonView.RPC(nameof(GameStartedRPC), RpcTarget.All, seed1, seed2);
    public void SendDrawCard(int actorNumber, int count, bool top) => photonView.RPC(nameof(DrawCardRPC), RpcTarget.All, actorNumber, count, top);

    public void SendEndTurn()
    {
        Debug.Log("->>>>>>>>>SendEndTurn");
        photonView.RPC(nameof(EndTurnRPC), RpcTarget.All);
    }
    public void SendSetActorTurn(int actorNumber) => photonView.RPC(nameof(SetActorTurnRPC), RpcTarget.All, actorNumber);
    public void SendDefuseUsed(int actorNumber, int explodeIndex) => photonView.RPC(nameof(DefuseUsedRPC), RpcTarget.All, actorNumber, explodeIndex);
    public void SendPlayerExploded(int actorNumber) => photonView.RPC(nameof(PlayerExplodedRPC), RpcTarget.All, actorNumber);
    public void SendPlayActionCard(int actorNumber, int cardType) => photonView.RPC(nameof(PlayActionCardRPC), RpcTarget.All, actorNumber, cardType);
    public void SendShuffleDeck(int randomSeed) => photonView.RPC(nameof(ShuffleDeckRPC), RpcTarget.All, randomSeed);
    public void SendRequestFavorCard(int from, int to) => photonView.RPC(nameof(RequestFavorCardRPC), RpcTarget.All, from, to);
    public void SendResponseFavorCard(int from, int to, int cardType) => photonView.RPC(nameof(ResponseFavorCardRPC), RpcTarget.All, from, to, cardType);
    public void SendAlterTheFutureCardsChanged(int actorNumber, int[] cards) => photonView.RPC(nameof(AlterTheFutureCardsChangedRPC), RpcTarget.All, actorNumber, cards);
    public void SendStealCardWith2CardCombo(int from, int to, int cardType) => photonView.RPC(nameof(StealCardWith2CardComboRPC), RpcTarget.All, from, to, cardType);

    [PunRPC] private void GameStartedRPC(int seed1, int seed2) => OnGameStarted?.Invoke(seed1, seed2);
    [PunRPC] private void DrawCardRPC(int actorNumber, int count, bool top) => OnCardDrawn?.Invoke(actorNumber, count, top);
    [PunRPC] private void EndTurnRPC() => OnEndTurn?.Invoke();
    [PunRPC] private void SetActorTurnRPC(int actor) => OnSetActorTurn?.Invoke(actor);
    [PunRPC] private void DefuseUsedRPC(int actorNumber, int explodeIndex) => OnDefuseUsed?.Invoke(actorNumber, explodeIndex);
    [PunRPC] private void PlayerExplodedRPC(int actorNumber) => OnPlayerExploded?.Invoke(actorNumber);
    [PunRPC] private void PlayActionCardRPC(int actorNumber, int cardType) => OnActionCardPlayed?.Invoke(actorNumber, cardType);
    [PunRPC] private void ShuffleDeckRPC(int seed) => OnShuffleDeck?.Invoke(seed);
    [PunRPC] private void RequestFavorCardRPC(int from, int to) => OnRequestFavorCard?.Invoke(from, to);
    [PunRPC] private void ResponseFavorCardRPC(int from, int to, int cardType) => OnResponseFavorCard?.Invoke(from, to, cardType);
    [PunRPC] private void AlterTheFutureCardsChangedRPC(int actorNumber, int[] cards) => OnAlterTheFutureCardsChanged?.Invoke(actorNumber, cards);
    [PunRPC] private void StealCardWith2CardComboRPC(int from, int to, int cardType) => OnStealCardWith2CardCombo?.Invoke(from, to, cardType);
}
