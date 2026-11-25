using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Alta;
using UnityEngine;
using GameList = Il2CppSystem.Collections.Generic;

namespace CustomSkillsAPI
{
    static class Plat
    {
        internal const string Name = "Quest";

        internal static bool IsServer()
        {
            return false;
        }

        static GameList.Dictionary<uint, ProfessionSkill> Skills()
        {
            HashedGeneralValue<ProfessionSkill>.CheckItems();
            return HashedGeneralValue<ProfessionSkill>.items;
        }

        internal static void SetGlyphs(ProfessionSkill skill, Sprite[] sprites)
        {
            if (sprites == null)
            {
                skill.practiceGlyphSprites = null;
                return;
            }
            Il2CppSystem.Array made = Il2CppSystem.Array.CreateInstance(Il2CppType.Of<Sprite>(), sprites.Length);
            Il2CppReferenceArray<Sprite> glyphs = new Il2CppReferenceArray<Sprite>(made.Pointer);
            for (int i = 0; i < sprites.Length; i++) glyphs[i] = sprites[i];
            skill.practiceGlyphSprites = glyphs;
        }

        internal static ProgressionSlot NewSlot(ProfessionSkillTree tree, ProfessionSkill skill)
        {
            GameList.List<ProgressionSlot> list = tree.Slots;
            ProgressionSlot like = list != null && list.Count > 0 ? list[0] : null;
            if (like == null) throw new InvalidOperationException("the " + tree.name + " tree has no slot to copy");
            Il2CppSystem.Object copy = like.MemberwiseClone();
            ProgressionSlot slot = copy == null ? null : copy.TryCast<ProgressionSlot>();
            if (slot == null) throw new InvalidOperationException("could not copy a slot of the " + tree.name + " tree");
            slot.Path = default(ProgressionPath);
            slot.skill = skill;
            slot.name = skill.name;
            slot.cost = 1u;
            slot.dependencies = new GameList.List<uint>();
            slot.hash = 0u;
            slot.indentLevel = -1;
            slot.position = Vector3.zero;
            slot.slotIcon = null;
            slot.inheritSlots = new GameList.List<ProgressionSlot>();
            return slot;
        }

        internal static void FillSlot(ProgressionSlot slot, string name, uint cost, List<uint> dependencies, uint hash, Vector3 position, Texture2D icon)
        {
            GameList.List<uint> list = null;
            if (dependencies != null)
            {
                list = new GameList.List<uint>();
                foreach (uint d in dependencies) list.Add(d);
            }
            slot.name = name;
            slot.cost = cost;
            slot.dependencies = list;
            slot.hash = hash;
            slot.position = position;
            if (icon != null) slot.slotIcon = icon;
        }

        internal static void MapSlot(ProfessionSkillTree tree, ProgressionSlot slot) { tree.slotMap[slot.Hash] = slot; }
        internal static int InheritCount(ProgressionSlot slot) { return slot.inheritSlots == null ? 0 : slot.inheritSlots.Count; }

        internal static string Join(GameList.List<uint> list)
        {
            List<string> parts = new List<string>();
            if (list != null) for (int i = 0; i < list.Count; i++) parts.Add(list[i].ToString(CultureInfo.InvariantCulture));
            return parts.Count == 0 ? "nothing" : string.Join(", ", parts.ToArray());
        }

        internal static string Missing()
        {
            var items = Skills();
            if (items == null || items.Count == 0) return "the skill list";
            if (ProfessionSkillTree.GetProfessionTree(ProgressionPath.Melee) == null) return "the skill trees";
            return null;
        }

        internal static void Register(ProfessionSkill skill)
        {
            Skills().Add(skill.Hash, skill);
        }

        internal static string Vec(Vector3 v)
        {
            return "(" + v.x.ToString("0.00", CultureInfo.InvariantCulture) + ", " + v.y.ToString("0.00", CultureInfo.InvariantCulture) + ", " + v.z.ToString("0.00", CultureInfo.InvariantCulture) + ")";
        }
    }
}
