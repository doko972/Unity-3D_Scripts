using UnityEngine;
using TMPro; // Nécessaire pour TextMeshPro
using System.Collections;

public class StarshipController : MonoBehaviour
{
    [Header("Speed Settings")]
    public float acceleration = 10f;        // Accélération normale
    public float maxSpeed = 30f;            // Vitesse maximale normale
    public float turboMultiplier = 2f;      // Multiplicateur de vitesse pour le turbo (Shift)

    [Header("Audio Settings")]
    public float audioFadeSpeed = 1f;       // Vitesse de réduction du volume audio

    [Header("Braking Settings")]
    public float brakeForce = 5f;          // Force de freinage pour ralentir rapidement

    [Header("Rotation Settings")]
    public float rotationSpeed = 2f;       // Sensibilité des rotations avec la souris
    public float rollSpeed = 50f;          // Vitesse du roulis (A/E)

    [Header("Vertical Movement")]
    public float verticalSpeed = 5f;       // Vitesse de montée/descente

    [Header("UI Elements")]
    public TMP_Text speedDisplay;           // Texte UI pour afficher la vitesse maximale
    public TMP_Text currentSpeedDisplay;    // Texte UI pour afficher la vitesse actuelle

    [Header("Audio")]
    public AudioSource audioSource;         // Source audio pour jouer le son
    public AudioClip accelerationSound;     // Clip sonore à jouer lors de l'accélération

    private float currentSpeed = 0f;        // Vitesse actuelle
    private bool isTurboActive = false;     // Indique si le turbo est actif
    private bool isBraking = false;         // Indique si le freinage est actif

    void Start()
    {
        // Verrouille le curseur pour permettre le contrôle avec la souris
        Cursor.lockState = CursorLockMode.Locked;

        // Configurer automatiquement l'AudioSource si elle n'est pas assignée
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                Debug.LogError("Aucun AudioSource trouvé sur l'objet. Veuillez en ajouter un !");
            }
        }

        // Vérifie si un son est assigné
        if (accelerationSound == null)
        {
            Debug.LogWarning("Aucun son d'accélération assigné.");
        }

        // Vérifie les éléments UI assignés
        if (speedDisplay == null)
        {
            Debug.LogWarning("Aucun élément UI assigné pour afficher la vitesse maximale.");
        }
        if (currentSpeedDisplay == null)
        {
            Debug.LogWarning("Aucun élément UI assigné pour afficher la vitesse actuelle.");
        }
    }

    void Update()
    {
        // Gestion des déplacements avant et arrière
        HandleForwardAndBackward();

        // Déplacement du vaisseau
        transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);

        // Gestion des rotations avec la souris
        HandleMouseRotation();

        // Gestion du roulis avec A/E
        HandleRoll();

        // Gestion du mouvement vertical (monter/descendre)
        HandleVerticalMovement();

        // Mise à jour des affichages UI
        UpdateSpeedDisplay();

        // Gestion du son lorsqu'aucune touche n'est pressée
        if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S))
        {
            HandleAudioFade();
        }
    }

    private void HandleForwardAndBackward()
    {
        // Vérifier si le turbo (Shift) est activé
        isTurboActive = Input.GetKey(KeyCode.LeftShift);

        // Calcul de la vitesse maximale effective
        float effectiveMaxSpeed = isTurboActive ? maxSpeed * turboMultiplier : maxSpeed;

        // Accélération en avant avec Z
        if (Input.GetKey(KeyCode.W))
        {
            currentSpeed = Mathf.Clamp(currentSpeed + acceleration * Time.deltaTime, 0, effectiveMaxSpeed);
            isBraking = false;

            // Joue ou ajuste le volume si Z est pressée
            PlayAudio(1f);
        }
        // Freiner beaucoup plus vite avec S
        else if (Input.GetKey(KeyCode.S))
        {
            currentSpeed = Mathf.Clamp(currentSpeed - brakeForce * Time.deltaTime, 0, effectiveMaxSpeed);
            isBraking = true;

            // Joue le son pour freiner avec volume réduit
            PlayAudio(0.5f);
        }
        // Décélération naturelle
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, acceleration * Time.deltaTime);
        }
    }

    private void HandleAudioFade()
    {
        if (audioSource != null && audioSource.volume > 0f)
        {
            audioSource.volume = Mathf.MoveTowards(audioSource.volume, 0f, audioFadeSpeed * Time.deltaTime);

            // Stoppe l'audio une fois le volume à zéro
            if (audioSource.volume == 0f && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
    }

    private void PlayAudio(float targetVolume)
    {
        if (audioSource != null && accelerationSound != null)
        {
            if (!audioSource.isPlaying)
            {
                audioSource.clip = accelerationSound;
                audioSource.Play();
            }
            audioSource.volume = Mathf.MoveTowards(audioSource.volume, targetVolume, audioFadeSpeed * Time.deltaTime);
        }
    }

    private void HandleMouseRotation()
    {
        float yaw = Input.GetAxis("Mouse X") * rotationSpeed;   // Déplacement gauche/droite
        float pitch = -Input.GetAxis("Mouse Y") * rotationSpeed; // Déplacement haut/bas

        // Appliquer la rotation à l'objet
        transform.Rotate(pitch, yaw, 0, Space.Self);
    }

    private void HandleRoll()
    {
        float roll = 0f;

        // Rouler avec A et E
        if (Input.GetKey(KeyCode.A)) roll = rollSpeed * Time.deltaTime;  // Rouler à gauche
        if (Input.GetKey(KeyCode.D)) roll = -rollSpeed * Time.deltaTime; // Rouler à droite

        // Appliquer le roulis
        transform.Rotate(0, 0, roll, Space.Self);
    }

    private void HandleVerticalMovement()
    {
        // Monter avec Espace
        if (Input.GetKey(KeyCode.Space))
        {
            transform.Translate(Vector3.up * verticalSpeed * Time.deltaTime);
        }
        // Descendre avec Ctrl
        else if (Input.GetKey(KeyCode.LeftControl))
        {
            transform.Translate(Vector3.down * verticalSpeed * Time.deltaTime);
        }
    }

    private void UpdateSpeedDisplay()
    {
        if (speedDisplay != null)
        {
            speedDisplay.text = "Max Speed: " + maxSpeed.ToString("F1");
        }

        if (currentSpeedDisplay != null)
        {
            currentSpeedDisplay.text = "Vitesse: " + currentSpeed.ToString("F1") + " Km/h";
        }
    }
}
