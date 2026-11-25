using System;
using System.Collections.Generic;
using Alta;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(CustomSkillsAPI.Core), "CustomSkillsAPI", CustomSkillsAPI.Port.Version, "CGNik, ported by Shadow", null)]
[assembly: MelonGame("Alta", "Att_Quest")]

namespace CustomSkillsAPI
{
    public class Core : MelonMod
    {
        public static event Action PreSetUpProgressionSlots = () => { };
        public static event Action SetUpProgressionSlots = () => { };
        public static event Action PostSetUpProgressionSlots = () => { };
        public static event Action PostAddProgressionSlots = () => { };
        public static event Action PostAddInherits = () => { };

        public static List<ProgressionSlot> progressionSlots = new List<ProgressionSlot>();

        public override void OnInitializeMelon()
        {
            Port.Init(LoggerInstance, RunSetup);
            LoggerInstance.Msg("Initialized.");
        }

        public override void OnLateInitializeMelon()
        {
            Port.Start("late start");
        }

        static void RunSetup()
        {
            Port.InvokeEach(PreSetUpProgressionSlots, nameof(PreSetUpProgressionSlots));

            Port.InvokeEach(SetUpProgressionSlots, nameof(SetUpProgressionSlots));

            Port.InvokeEach(PostSetUpProgressionSlots, nameof(PostSetUpProgressionSlots));

            foreach (ProgressionSlot progressionSlot in progressionSlots)
            {
                ProfessionSkillTree professionSkillTree = ProfessionSkillTree.GetProfessionTree(progressionSlot.Path);

                Plat.MapSlot(professionSkillTree, progressionSlot);
                professionSkillTree.Slots.Add(progressionSlot);
            }

            Port.InvokeEach(PostAddProgressionSlots, nameof(PostAddProgressionSlots));

            foreach (ProgressionSlot progressionSlot in progressionSlots)
            {
                progressionSlot.AddInherit(ProfessionSkillTree.GetProfessionTree(progressionSlot.Path));
            }

            Port.InvokeEach(PostAddInherits, nameof(PostAddInherits));

            Port.Report(progressionSlots);
        }

        public static void FixProfessionSkillInspectorValues(ProfessionSkill professionSkill, Sprite[] sprites)
        {
            Plat.SetGlyphs(professionSkill, sprites);
        }

        public static void RegisterProfessionSkill(ProfessionSkill professionSkill)
        {
            Plat.Register(professionSkill);
            Port.Registered(professionSkill);
        }

        public static void AddProfessionSkill(ProfessionSkill professionSkill, Sprite[] sprites)
        {
            FixProfessionSkillInspectorValues(professionSkill, sprites);
            RegisterProfessionSkill(professionSkill);
        }

        public static ProgressionSlot AddProgressionSlot(ProfessionSkill professionSkill, string name, uint cost, List<uint> dependencies, uint hash, Vector3 position, ProgressionPath progressionPath, Texture2D slotIcon = null)
        {
            ProfessionSkillTree professionSkillTree = ProfessionSkillTree.GetProfessionTree(progressionPath);
            ProgressionSlot progressionSlot = Plat.NewSlot(professionSkillTree, professionSkill);
            Plat.FillSlot(progressionSlot, name, cost, dependencies, hash, position, slotIcon);
            progressionSlot.Validate(professionSkillTree);
            progressionSlots.Add(progressionSlot);
            return progressionSlot;
        }
    }
}
