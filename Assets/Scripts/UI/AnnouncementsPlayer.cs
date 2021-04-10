using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum AnnouncementTypes
{
    NewTurn,
    PlayerTurn
}

public class AnnouncementsPlayer : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioPlayer;
    [SerializeField] private AudioClip newTurnSound;

    [SerializeField] private Material panelBackground;
    [SerializeField] private TMP_Text panelText;

    public void PlayAnnouncement(AnnouncementTypes type, PlayerController player = null)
    {
        Dictionary<AnnouncementTypes, string> animations = new Dictionary<AnnouncementTypes, string>()
        {
            { AnnouncementTypes.NewTurn, "NewTurn" },
            { AnnouncementTypes.PlayerTurn, "PlayerTurn" }
        };

        if (player != null)
        {
            panelBackground.SetColor("_Color", player.PlayerColor);
            panelText.text = player.PlayerName;
            audioPlayer.PlayOneShot(player.newTurnSound);
        } else
        {
            panelBackground.SetColor("_Color", Color.white);
            panelText.text = "New Turn";
            audioPlayer.PlayOneShot(newTurnSound);
        }

        animator.SetTrigger(animations[type]);
    }
}
