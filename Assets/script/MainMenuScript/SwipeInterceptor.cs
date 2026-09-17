using UnityEngine;
using UnityEngine.EventSystems;

public class SwipeInterceptor : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector2 _startDragPosition;
    private bool _swipeIdentified = false;

    [SerializeField] private MainMenuController _menuController;

    private void Start()
    {
        if (_menuController == null)
        {
            _menuController = FindObjectOfType<MainMenuController>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _startDragPosition = eventData.position;
        _swipeIdentified = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_swipeIdentified) return;

        Vector2 currentDelta = eventData.position - _startDragPosition;
        float horizontalDistance = Mathf.Abs(currentDelta.x);
        float verticalDistance = Mathf.Abs(currentDelta.y);

        if (horizontalDistance > 10f || verticalDistance > 10f)
        {
            _swipeIdentified = true;

            if (horizontalDistance > verticalDistance)
            {
                bool toRight = (currentDelta.x < 0);
                
                if (_menuController != null)
                {
                    _menuController.RequestSwipe(toRight);
                }

                GetComponent<UnityEngine.UI.ScrollRect>().enabled = false;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        GetComponent<UnityEngine.UI.ScrollRect>().enabled = true;
    }
}