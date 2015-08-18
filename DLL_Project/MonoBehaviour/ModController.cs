﻿using System;
using System.Collections.Generic;
using System.Linq;

using RimWorld;
using UnityEngine;
using Verse;

namespace CommunityCoreLibrary
{

    public class ModController : MonoBehaviour
    {
        
        public readonly string              GameObjectName = "Community Core Library";

        public static List< AdvancedResearchDef > AdvancedResearch;

        public static Version               CCLVersion;
        List< ModHelperDef >                ModHelperDefs;

        public void                         Start()
        {
            enabled = false;

            // Check versions of mods and throw error to the user if the
            // mod version requirement is higher than the installed version
            GetCCLVersion();

            ModHelperDefs = DefDatabase< ModHelperDef >.AllDefs.ToList();

            if( ( ModHelperDefs != null )&&
                ( ModHelperDefs.Count > 0 ) )
            {
                ValidateMods();
            }

            // Validate advanced research defs
            CheckAdvancedResearch();

            Log.Message( "Community Core Library :: Initialized" );

            enabled = true;
        }

        public void                         FixedUpdate()
        {
            if( !ReadyForInjection() )
            {
                return;
            }

            InjectComponents();
        }

        #region Versioning

        void                                GetCCLVersion ()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            CCLVersion = assembly.GetName().Version;
#if DEBUG
            Log.Message( "Community Core Library v" + CCLVersion + " (debug)" );
#else
            Log.Message( "Community Core Library v" + CCLVersion );
#endif
        }

        void                                ValidateMods()
        {
            for( int i = 0; i < ModHelperDefs.Count; i++ ){
                var ModHelperDef = ModHelperDefs[ i ];
                if( !ModHelperDef.IsValid )
                {
                    // Don't do anything special with broken mods
                    ModHelperDefs.Remove( ModHelperDef );
                    continue;
                }
            }

        }

        #endregion

        #region Research Verification

        void                                CheckAdvancedResearch()
        {
            // Make sure the hidden research exists
            if( Research.Locker == null )
            {
                Log.Error( "Community Core Library :: Advanced Research :: Missing Research.Locker!" );
            }

            // Get the [advanced] research defs
            AdvancedResearch = DefDatabase< AdvancedResearchDef >.AllDefs.OrderBy( a => a.Priority ).ToList();

            if( ( AdvancedResearch == null )&&
                ( AdvancedResearch.Count == 0 ) )
            {
                return;
            }

            // Validate each advanced research def
            for( int i = 0; i < AdvancedResearch.Count; i++ ){
                var Advanced = AdvancedResearch[ i ];

                if( !Advanced.IsValid )
                {
                    // Remove projects with errors from list of usable projects
                    AdvancedResearch.Remove( Advanced );
                    continue;
                }
            }

            // All advanced research checked, no log errors means it's all good
        }

        #endregion

        #region Component Injection

        bool                                ReadyForInjection()
        {
            return (
                ( Find.Map != null )&&
                ( Find.Map.components != null )
            );
        }

        void                                InjectComponents()
        {
            // Check that all the map components are injected into the game
            foreach( var ModHelperDef in ModHelperDefs )
            {
                if( !ModHelperDef.IsInjected )
                {
                    Log.Message( "Community Core Library :: Injecting MapComponents for " + ModHelperDef.ModName );
                    ModHelperDef.Inject();
                }
            }
        }

        #endregion

    }

}
