using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum AnnouncementTypes
{
    NewTurn,
    PlayerTurn,
    SuddenDeath
}

public class AnnouncementsPlayer : MonoBehaviour
{
    [SerializeField] private GameplayUI gameplayUI;
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioPlayer;
    [SerializeField] private AudioClip newTurnSound;

    [SerializeField] private Material panelBackground;
    [SerializeField] private TMP_Text panelText;

    public IEnumerator PlayAnnouncement(AnnouncementTypes type, PlayerController player = null)
    {
        Dictionary<AnnouncementTypes, string> animations = new Dictionary<AnnouncementTypes, string>()
        {
            { AnnouncementTypes.NewTurn, "NewTurn" },
            { AnnouncementTypes.PlayerTurn, "PlayerTurn" },
            { AnnouncementTypes.SuddenDeath, "PlayerTurn" }
        };

        if (player != null)
        {
            panelBackground.SetColor("_Color", player.PlayerColor);
            panelText.text = player.PlayerName;
            audioPlayer.PlayOneShot(player.newTurnSound);
        } else
        {
            panelBackground.SetColor("_Color", type == AnnouncementTypes.SuddenDeath ? new Color(1f, 0.5f, 0f) : Color.white);
            panelText.text = type == AnnouncementTypes.SuddenDeath ? "Sudden Death" : "New Turn";
            audioPlayer.PlayOneShot(newTurnSound);
        }

        StartCoroutine(gameplayUI.SetInteractable(false));
        animator.SetTrigger(animations[type]);
        yield return StartCoroutine(WaitForAnimationEnd());
        StartCoroutine(gameplayUI.SetInteractable(true));
        yield return new WaitForSeconds(0.2f);
    }

    private IEnumerator WaitForAnimationEnd()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
    }
}
