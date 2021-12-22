using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LGM.UI
{
    public class CardRemover : MonoBehaviour
    {
        public void DelayedRemoveCard(Player player, string cardName, int frames = 10)
        {
            StartCoroutine(RemoveCard(player, cardName, frames));
        }

        IEnumerator RemoveCard(Player player, string cardName, int frames = 10)
        {
            yield return StartCoroutine(UIHandlerExtensions.WaitForFrames(frames));

            for (int i = player.data.currentCards.Count - 1; i >= 0; i--)
            {
                if (player.data.currentCards[i].cardName == cardName)
                {
                    ModdingUtils.Utils.Cards.instance.RemoveCardFromPlayer(player, i);
                    break;
                }
            }
        }
    }
}
