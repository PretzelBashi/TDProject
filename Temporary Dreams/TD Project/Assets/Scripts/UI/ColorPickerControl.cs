using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Collections.Generic;

public class ColorPickerControl : MonoBehaviour
{
    //Este codigo viene del tutorial "In-Game Colour Picker - Unity Tutorial (http://youtube.com/watch?v=otDHGmncBQY)"
    public float currentHue, currentSat, currentVal;
    public Color currentColour;

    [SerializeField]
    private RawImage hueImage, satValImage, outputImage;
    [SerializeField]
    private Slider hueSlider;
    [SerializeField]
    private TMP_InputField hexInputField;
    private Texture2D hueTexture, svTexture, outputTexture;

    List<SkinnedMeshRenderer> materiales;

    [SerializeField]
    Transform modeloJugador;

    

    private void Start()
    {
        currentColour = new Color();
        materiales = new List<SkinnedMeshRenderer>();
        materiales.Add(modeloJugador.Find("pelo").GetComponent<SkinnedMeshRenderer>());
        materiales.Add(modeloJugador.Find("pecho").GetComponent<SkinnedMeshRenderer>());
        materiales.Add(modeloJugador.Find("piernas").GetComponent<SkinnedMeshRenderer>());
        materiales.Add(modeloJugador.Find("zapatos").GetComponent<SkinnedMeshRenderer>());

        CreateHueImage();
        CreateSVImage();
        CreateOutputImage();
        UpdateOutputImage();
    }
    private void CreateHueImage()
    {
        hueTexture = new Texture2D(1, 16);
        hueTexture.wrapMode = TextureWrapMode.Clamp;
        hueTexture.name = "HueTexture";

        for (int i = 0; i < hueTexture.height; i++)
        {
            hueTexture.SetPixel(0, i, Color.HSVToRGB((float)i / hueTexture.height, 1, 1f));
        }

        hueTexture.Apply();
        currentHue = 0;

        hueImage.texture = hueTexture;
    }

    private void CreateSVImage()
    {
        svTexture = new Texture2D(16, 16);
        svTexture.wrapMode = TextureWrapMode.Clamp;
        svTexture.name = "SatValTexture";

        for(int y = 0;y < svTexture.height; y++)
        {
            for (int x = 0; x < svTexture.height; x++)
            {
                svTexture.SetPixel(x, y, Color.HSVToRGB(currentHue, (float)x / svTexture.width, (float)y / svTexture.height));
            }
        }

        svTexture.Apply();
        currentSat = 0;
        currentVal = 0;

        satValImage.texture = svTexture;
    }

    private void CreateOutputImage()
    {
        outputTexture = new Texture2D (1, 16);
        outputTexture.wrapMode = TextureWrapMode.Clamp;
        outputTexture.name = "OutputTexture";

        Color currentColour = Color.HSVToRGB(currentHue, currentSat, currentVal);

        for(int i = 0; i< outputTexture.height; i++)
        {
            outputTexture.SetPixel(0, i, currentColour);
        }
        outputTexture.Apply();

        outputImage.texture = outputTexture;
    }

    private void UpdateOutputImage()
    {
        currentColour = Color.HSVToRGB(currentHue, currentSat, currentVal);

        for(int i = 0; i<outputTexture.height; i++)
        {
            outputTexture.SetPixel(0,i,currentColour);
        }
        outputTexture.Apply();

        hexInputField.text = ColorUtility.ToHtmlStringRGB(currentColour);

        for(int i = 0; i < materiales.Count(); i++)
        {
            foreach(Material material in materiales[i].materials)
            {
                switch (material.name)
                {
                    case "pelo (Instance)":
                        material.color = currentColour;
                        break;
                    case "traje (Instance)":
                        material.color = currentColour;
                        break;

                    case "trageP2 (Instance)":
                        material.color = new Color(
                            currentColour.r * 0.65f,
                            currentColour.g * 0.65f,
                            currentColour.b * 0.65f,
                            currentColour.a
                        );
                        break;
                }
            }
        }
    }

    public void SetSV(float S, float V)
    {
        currentSat = S; currentVal = V;
        UpdateOutputImage();
    }

    public void UpdateSVImage()
    {
        currentHue = hueSlider.value;

        for (int y = 0; y < svTexture.height; y++)
        {
            for (int x = 0; x < svTexture.height; x++)
            {
                svTexture.SetPixel(x, y, Color.HSVToRGB(currentHue, (float)x / svTexture.width, (float)y / svTexture.height));
            }
        }

        svTexture.Apply();

        UpdateOutputImage();
    }

    public void OnTextInput()
    {
        if (hexInputField.text.Length < 6) { return; }

        Color newCol;

        if (ColorUtility.TryParseHtmlString("#" + hexInputField.text, out newCol))
            Color.RGBToHSV(newCol, out currentHue, out currentSat, out currentVal);

        hueSlider.value = currentHue;

        hexInputField.text = "";

        UpdateOutputImage();
    }
}
