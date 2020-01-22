﻿using System;

using UnityEngine;
using UnityEngine.UI;

using Zilon.Core.Props;

namespace Assets.Zilon.Scripts.Models.Modals
{
    public class PropItemViewModelBase : MonoBehaviour
    {
        public Text CountText;
        public Text DurableStatusText;
        public Image IconImage;

        public string Sid;

        public event EventHandler Click;
        public event EventHandler MouseEnter;
        public event EventHandler MouseExit;

        public IProp Prop { get; protected set; }

        public Vector3 Position => GetComponent<RectTransform>().position;

        public void Click_Handler()
        {
            Click?.Invoke(this, new EventArgs());
        }

        public void OnMouseEnter()
        {
            MouseEnter?.Invoke(this, new EventArgs());
        }

        public void OnMouseExit()
        {
            MouseExit?.Invoke(this, new EventArgs());
        }

        public void UpdateProp()
        {
            if (Prop == null)
            {
                throw new InvalidOperationException("Предмет не задан");
            }

            switch (Prop)
            {
                case Resource resource:
                    UpdateResource(resource);
                    break;

                case Equipment equipment:
                    UpdateEquipment(equipment);
                    break;

                default:
                    throw new InvalidOperationException($"Тип предмета {Prop?.GetType()?.Name} не поддерживается");

            }

            Sid = Prop?.Scheme?.Sid;

            var iconSprite = CalcIcon(Prop);

            IconImage.sprite = iconSprite;
        }

        private void UpdateEquipment(Equipment equipment)
        {
            CountText.gameObject.SetActive(false);

            if (equipment.Durable.Value <= 0)
            {
                DurableStatusText.gameObject.SetActive(true);
                DurableStatusText.text = "B";
            }
            else
            {
                DurableStatusText.gameObject.SetActive(false);
            }
        }

        private void UpdateResource(Resource resource)
        {
            CountText.gameObject.SetActive(true);
            CountText.text = $"x{resource.Count}";

            DurableStatusText.gameObject.SetActive(false);
        }

        private Sprite CalcIcon(IProp prop)
        {
            var schemeSid = prop.Scheme.Sid;
            if (prop.Scheme.IsMimicFor != null)
            {
                schemeSid = prop.Scheme.IsMimicFor;
            }

            var iconSprite = Resources.Load<Sprite>($"Icons/props/{schemeSid}");
            return iconSprite;
        }
    }
}
