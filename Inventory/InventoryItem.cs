// <copyright file="InventoryItem.cs" company="Ensage">
//    Copyright (c) 2017 Ensage.
// </copyright>

namespace Ensage.SDK.Inventory
{
    using System;

    using Ensage.Common.Enums;

    public class InventoryItem : IEquatable<InventoryItem>
    {
        public InventoryItem(Item item)
        {
            this.Id = (ItemId)item.Id;
            this.Item = item;
        }

        public ItemId Id { get; }

        public bool IsValid => this.Item != null && this.Item.IsValid;

        public Item Item { get; }

        public static bool operator ==(InventoryItem left, InventoryItem right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(InventoryItem left, InventoryItem right)
        {
            return !Equals(left, right);
        }

        public bool Equals(InventoryItem other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return this.Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return this.Equals((InventoryItem)obj);
        }

        public override int GetHashCode()
        {
            if (this.IsValid)
            {
                return (int)this.Id ^ this.Item.GetHashCode();
            }

            return (int)this.Id;
        }
    }
}