using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DivIt.Utils;

namespace DivIt.PlayVRoom.Managers
{
    public class LayerManager : Singleton<LayerManager>
    {
        #region Private Variables - Serialized

        [Header("----------------------------------------")]
        [SerializeField] private string _layerDefault = "";
        [SerializeField] private string _layerVrHands = "";
        [SerializeField] private string _layerVrInteractable = "";
        [SerializeField] private string _layerHandColliders = "";
        [SerializeField] private string _layerVrPlayer = "";
        [Space(20)]
        [SerializeField] private string _layerGrenadeExplosion = "";

        #endregion

        #region Getters

        public string LayerDefault
        {
            get
            {
                return _layerDefault;
            }
        }
        public string LayerVrHands
        {
            get
            {
                return _layerVrHands;
            }
        }
        public string LayerVrInteractable
        {
            get
            {
                return _layerVrInteractable;
            }
        }
        public string LayerHandColliders
        {
            get
            {
                return _layerHandColliders;
            }
        }
        public string LayerVrPlayer
        {
            get
            {
                return _layerVrPlayer;
            }
        }
        public string LayerGrenadeExplosion
        {
            get
            {
                return _layerGrenadeExplosion;
            }
        }

        #endregion

        public override void Awake()
        {
            base.Awake();

            InteractionLayerSetup();            
        }

        /// <summary>
        /// Sets up the most important low level layers and collisions
        /// required for interactions with VR Interactable objects and hands mostly.
        /// </summary>
        private void InteractionLayerSetup()
        {
            //Interaction layers setup
            int nameToLayerDefault = LayerMask.NameToLayer(_layerDefault);
            int nameToLayerVrHands = LayerMask.NameToLayer(_layerVrHands);
            int nameToLayerVrInteractable = LayerMask.NameToLayer(_layerVrInteractable);
            int nameToLayerHandColliders = LayerMask.NameToLayer(_layerHandColliders);

            //Remove all layer collision from above layers
            for (int i = 0; i <= 31; i++) //Unity supports 31 layers
            {
                Physics.IgnoreLayerCollision(i, nameToLayerVrHands, true);
                Physics.IgnoreLayerCollision(i, nameToLayerVrInteractable, true);
                Physics.IgnoreLayerCollision(i, nameToLayerHandColliders, true);
            }

            //Allow interaction between:
            //VrHands and VrInteractable
            Physics.IgnoreLayerCollision(nameToLayerVrHands, nameToLayerVrInteractable, false);
            //VrInteractable and VrInteractable
            Physics.IgnoreLayerCollision(nameToLayerVrInteractable, nameToLayerVrInteractable, false);
            //HandColliders and VrInteractable
            Physics.IgnoreLayerCollision(nameToLayerHandColliders, nameToLayerVrInteractable, false);
            //HandColliders and HandColliders
            Physics.IgnoreLayerCollision(nameToLayerHandColliders, nameToLayerHandColliders, false);
            //Default and VrInteractable
            Physics.IgnoreLayerCollision(nameToLayerDefault, nameToLayerVrInteractable, false);
        }
    } 
}