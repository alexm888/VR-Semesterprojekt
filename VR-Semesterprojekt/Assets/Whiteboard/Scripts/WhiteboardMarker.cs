using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WhiteboardMarker : MonoBehaviour
{
    [SerializeField] private Transform _tip;

    // Hier Dicke Marker-Strich anpassen
    [SerializeField] private int _penSize = 5;

    private Renderer _renderer;
    private Color[] _colors;
    private float _tipHeight;

    private RaycastHit _touch;
    private Whiteboard _whiteboard;
    private Vector2 _touchPos, _lastTouchPos;
    private bool _touchedLastFrame;
    private Quaternion _lastTouchRot;

    void Start()
    {
        _renderer = _tip.GetComponent<Renderer>();
        _colors = Enumerable.Repeat(_renderer.material.color, _penSize * _penSize).ToArray();
        _tipHeight = _tip.localScale.y;

    }

    void Update()
    {
        Draw();
    }


    private void Draw()
    {
        if (Physics.Raycast(_tip.position, transform.up, out _touch, _tipHeight))
        {
            // Damit nur auf Whiteboard geschrieben werden kann
            if (_touch.transform.CompareTag("Whiteboard"))
            {
                if (_whiteboard == null)
                {
                    _whiteboard = _touch.transform.GetComponent<Whiteboard>();
                }

                _touchPos = new Vector2(_touch.textureCoord.x, _touch.textureCoord.y);

                // Touch Position auf Whiteboard Texturgr��e konvertieren
                var x = (int)(_touchPos.x * _whiteboard.textureSize.x - (_penSize / 2));
                var y = (int)(_touchPos.y * _whiteboard.textureSize.y - (_penSize / 2));

                // Exit wenn Position au�erhalb des Whiteboards ist
                if ((y < 0) || (y > _whiteboard.textureSize.y) || (x < 0) || (x > _whiteboard.textureSize.x)) return;

                if (_touchedLastFrame)
                {
                    // Initialpunkt wo Whiteboard zum ersten Mal ber�hrt wurde
                    _whiteboard.texture.SetPixels(x, y, _penSize, _penSize, _colors);

                    // Interpolation zwischen letztem und aktuellem Punkt
                    for (float f = 0.07f; f < 1.00f; f+= 0.01f)
                    {
                        var lerpX = (int)Mathf.Lerp(_lastTouchPos.x, x, f);
                        var lerpY = (int)Mathf.Lerp(_lastTouchPos.y, y, f);
                        _whiteboard.texture.SetPixels(lerpX, lerpY, _penSize, _penSize, _colors);

                    }

                    // Rotation sperren, damit Stift sicht nicht weiter dreht und "realer" anf�hlt
                    transform.rotation = _lastTouchRot;

                    _whiteboard.texture.Apply();
                }

                // Caching f�r n�chsten Frame
                _lastTouchPos = new Vector2(x, y);
                _lastTouchRot = transform.rotation;
                _touchedLastFrame = true;
                return;
            }
        }

        // uncache Whiteboard
        _whiteboard = null;
        _touchedLastFrame = false;
    }

}
