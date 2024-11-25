using UnityEngine;

public class EmotionController : MonoBehaviour
{
    [Header("Emotion Sprites")]
    [SerializeField] private Sprite angrySprite;
    [SerializeField] private Sprite cheerfulSprite;
    [SerializeField] private Sprite happySprite;
    [SerializeField] private Sprite supportiveSprite;
    [SerializeField] private Sprite surprisedSprite;
    [SerializeField] private Sprite upsetSprite;

    private SpriteRenderer _emotionSpriteRenderer;

    private void Awake()
    {
        _emotionSpriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    public void Angry()
    {
        _emotionSpriteRenderer.sprite = angrySprite;
    }
    
    public void Cheerful()
    {
        _emotionSpriteRenderer.sprite = cheerfulSprite;
    }
    
    public void Happy()
    {
        _emotionSpriteRenderer.sprite = happySprite;
    }
    
    public void Supportive()
    {
        _emotionSpriteRenderer.sprite = supportiveSprite;
    }
    
    public void Surprised()
    {
        _emotionSpriteRenderer.sprite = surprisedSprite;
    }
    
    public void Upset()
    {
        _emotionSpriteRenderer.sprite = upsetSprite;
    }
    
    // No emotion
    public void Default()
    {
        _emotionSpriteRenderer.sprite = null;
    }
}