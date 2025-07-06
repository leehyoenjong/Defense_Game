using UnityEngine;

public class UI_Probability : MonoBehaviour
{
    [SerializeField] GameObject _parent;
    [SerializeField] GameObject _probabilityslot;

    public void Setting(int gachaid)
    {
        var gachadata = DataManager.instance.GetGachaTable().GetGachaData(gachaid);

        var maxcount = gachadata.Count;
        for (int i = 0; i < maxcount; i++)
        {
            var slot = Instantiate(_probabilityslot, _parent.transform).GetComponent<UI_Probability_Slot>();
            slot.Setting(gachadata[i]);
        }
    }
}
