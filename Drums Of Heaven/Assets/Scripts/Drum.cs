using UnityEngine;

public class Drum : MonoBehaviour
{
    [SerializeField] private MeshRenderer m_DrumPad1;
    [SerializeField] private MeshRenderer m_DrumPad2;
    [SerializeField] private MeshRenderer m_DrumPad3;
    [SerializeField] private MeshRenderer m_DrumPad4;
    [SerializeField] private MeshRenderer m_DrumPad5;
    [SerializeField] private MeshRenderer m_DrumPad6;
    public System.Action<int> onDrumPadTapped;

    private void Update()
    {
        UpdateInputs();
    }

    private void UpdateInputs()
    {
        if (Input.GetKeyDown(KeyCode.Keypad4))
        {
            m_DrumPad1.material.color = Color.white;
            onDrumPadTapped?.Invoke(0);
        }

        if (Input.GetKeyUp(KeyCode.Keypad4))
        {
            m_DrumPad1.material.color = Color.black;
        }

        if (Input.GetKeyDown(KeyCode.Keypad5))
        {
            m_DrumPad2.material.color = Color.white;
            onDrumPadTapped?.Invoke(1);
        }

        if (Input.GetKeyUp(KeyCode.Keypad5))
        {
            m_DrumPad2.material.color = Color.black;
        }

        if (Input.GetKeyDown(KeyCode.Keypad6))
        {
            m_DrumPad3.material.color = Color.white;
            onDrumPadTapped?.Invoke(2);
        }

        if (Input.GetKeyUp(KeyCode.Keypad6))
        {
            m_DrumPad3.material.color = Color.black;
        }

        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            m_DrumPad4.material.color = Color.white;
            onDrumPadTapped?.Invoke(3);
        }

        if (Input.GetKeyUp(KeyCode.Keypad1))
        {
            m_DrumPad4.material.color = Color.black;
        }

        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            m_DrumPad5.material.color = Color.white;
            onDrumPadTapped?.Invoke(4);
        }

        if (Input.GetKeyUp(KeyCode.Keypad2))
        {
            m_DrumPad5.material.color = Color.black;
        }

        if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            m_DrumPad6.material.color = Color.white;
            onDrumPadTapped?.Invoke(5);
        }

        if (Input.GetKeyUp(KeyCode.Keypad3))
        {
            m_DrumPad6.material.color = Color.black;
        }
    }


}
