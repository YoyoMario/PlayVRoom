using DivIt.Utils;
using UnityEngine;

namespace DivIt.PlayVRoom.Managers
{
    public enum DecalType : byte
    {
        Brick = 0,
        Metal = 1,
        Dirt = 2
    }

    [System.Serializable]
    public struct DecalObjects
    {
        public DecalType Type;
        public GameObject Prefab;
        public GameObject[] PooledDecals;
        public int CurrentIndex;
        public Vector3 OriginalLocalScale;

        public void Initialize(int amount, Transform parent)
        {
            PooledDecals = new GameObject[amount];
            for (int i = 0; i < amount; i++)
            {
                PooledDecals[i] = GameObject.Instantiate(Prefab, parent);
                PooledDecals[i].SetActive(false);
                OriginalLocalScale = PooledDecals[i].transform.localScale;
            }
        }

        public void SetDecalPosition(Vector3 position, Quaternion rotation, Transform parent)
        {
            if (!PooledDecals[CurrentIndex].activeInHierarchy)
            {
                PooledDecals[CurrentIndex].SetActive(true);
            }
            PooledDecals[CurrentIndex].transform.position = position;
            PooledDecals[CurrentIndex].transform.rotation = rotation;
            CurrentIndex++;
            if (CurrentIndex == PooledDecals.Length)
            {
                CurrentIndex = 0;
            }
        }
    }

    public class DecalManager : Singleton<DecalManager>
    {
        [SerializeField] private DecalObjects _decalsBrick = default;
        [SerializeField] private DecalObjects _decalsMetal = default;
        [SerializeField] private DecalObjects _decalsDirt = default;
        [SerializeField] private int AmountOfDecals = 10;
        [SerializeField] private float _aboveSurfaceAmount = 0.01f;

        public override void Awake()
        {
            base.Awake();

            _decalsBrick.Initialize(AmountOfDecals, transform);
        }

        public void ShowDecal(DecalType decalType, Vector3 position, Vector3 hitNormalvector, Transform hitTransform)
        {
            Vector3 recalculatedPosition = position + (hitNormalvector * _aboveSurfaceAmount);
            Quaternion recalculatedRotation = Quaternion.LookRotation(hitNormalvector);

            switch (decalType)
            {
                case DecalType.Brick:
                    _decalsBrick.SetDecalPosition(recalculatedPosition, recalculatedRotation, hitTransform);
                    break;
                case DecalType.Metal:
                    _decalsMetal.SetDecalPosition(recalculatedPosition, recalculatedRotation, hitTransform);
                    break;
                case DecalType.Dirt:
                    _decalsDirt.SetDecalPosition(recalculatedPosition, recalculatedRotation, hitTransform);
                    break;
            }
        }
    }
}
