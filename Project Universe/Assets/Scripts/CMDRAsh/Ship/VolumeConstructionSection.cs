using ProjectUniverse.Base;
using ProjectUniverse.Data.Libraries.Definitions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectUniverse.Environment.Volumes
{
    public class VolumeConstructionSection : IConstructible
    {
        [SerializeField] private GameObject CON00;
        [SerializeField] private GameObject CON01;
        [SerializeField] private GameObject COMPLETE;
        //Sub/machines have one stage and are IConstructible
        [SerializeField] private bool machineSection;
        [SerializeField] private SectionType thisSectionType;
        // stage 0 has been completed
        [SerializeField] private bool stage0;
        // stage 1 has been completed
        [SerializeField] private bool stage1;
        // the section is fully built. Will not trigger for machines b/c the final stage is independent?
        [SerializeField] private bool complete;

        public enum SectionType
        {
            Ceiling,
            Floor,
            Wall,
            Door,
            Breakers,
            Lights,
            Ducts,
            Pipes,
            Overhead
        }

        public SectionType ThisSectionType
        {
            get
            {
                return thisSectionType;
            }
        }

        public bool MachineSection
        {
            get
            {
                return machineSection;
            }
        }
        public bool Stage0
        {
            get { return stage0; }
            set { stage0 = value; }
        }
        public bool Stage1
        {
            get { return stage1; }
            set { stage1 = value; }
        }
        public bool Complete
        {
            get { return complete; }
            set { complete = value; }
        }
        public GameObject CON00GameObject
        {
            get { return CON00; }
        }
        public GameObject CON01GameObject
        {
            get { return CON01; }
        }
        public GameObject CompletedGameObject
        {
            get { return COMPLETE; }
        }

        public void UpdateStageLogic(bool logic)
        {
            if (logic)
            {
                if (!Stage0)
                {
                    Stage0 = true;
                }
                else if (CON01 != null && !Stage1)
                {
                    Stage1 = true;
                }
                else if (!Complete)
                {
                    Complete = true;
                }
            }
        }

        public void BouncedMessageReceiver(params object[] data)
        {
            MachineMessageReceiver(data);
        }

        protected override void ProcessDamageToComponents()
        {

        }
    }
}