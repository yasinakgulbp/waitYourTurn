using System.Linq;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using WaitYourTurn.Combat;
using WaitYourTurn.Enemies;
using WaitYourTurn.Navigation;
using WaitYourTurn.Player;
using WaitYourTurn.Run;
using WaitYourTurn.Sandbox;
using WaitYourTurn.Train;

namespace WaitYourTurn.Editor
{
    public static class TrainIntegrationBuilder
    {
        public const string ScenePath = "Assets/Game/Scenes/TrainIntegration.unity";
        private const string NavFolder = "Assets/Game/Content/IntegrationNavigation";
        [MenuItem("Wait Your Turn/Integration/Build One Wagon %#F4")]
        public static void BuildOne() => Build(1);
        [MenuItem("Wait Your Turn/Integration/Build Two Wagons %#F5")]
        public static void BuildTwo() => Build(2);
        [MenuItem("Wait Your Turn/Integration/Build Five Wagons %#F3")]
        public static void BuildFive() => Build(5);
        private static void Build(int count)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var lab = EditorSceneManager.OpenScene("Assets/Game/Scenes/TrainSandbox.unity");
            var oldRun = Object.FindAnyObjectByType<RunDriver>();
            var hero = Object.Instantiate(oldRun.Player.gameObject);
            var template = Object.Instantiate(Object.FindObjectsByType<EnemyBrain>(FindObjectsInactive.Include).First().gameObject);
            var reference = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene 2.unity", OpenSceneMode.Additive);
            var source = reference.GetRootGameObjects().First(x => x.name == "Vagons").transform.GetChild(0);
            var originalCamera = reference.GetRootGameObjects().First(x => x.name == "Main Camera").GetComponent<Camera>();
            var sourceBody = source.GetComponentsInChildren<Renderer>().First(x => x.name == "main").bounds;
            Vector3 origin = new Vector3(sourceBody.center.x, 0, sourceBody.center.z);
            var doorBounds = source.GetComponentsInChildren<Renderer>().Where(x => x.transform.parent.name == "DOORS")
                .Select(x => x.bounds).OrderBy(x => x.center.x).ThenBy(x => x.center.z).ToArray();
            if (doorBounds.Length != 6) throw new System.InvalidOperationException("Reference wagon must contain six door meshes.");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            SceneManager.MoveGameObjectToScene(hero, scene); SceneManager.MoveGameObjectToScene(template, scene);
            hero.name = "Player - shared M3/M5 mechanics"; template.name = "Bounded enemy template"; template.SetActive(false);
            var player = hero.GetComponent<HealthComponent>();
            int agentType = EnsureAgentType();
            template.GetComponent<NavMeshAgent>().agentTypeID = agentType;
            var registry = new GameObject("Live target registry").AddComponent<TargetRegistry>();
            var camera = new GameObject("Main Camera - prototype player follow").AddComponent<Camera>(); camera.tag = "MainCamera";
            camera.gameObject.AddComponent<AudioListener>(); camera.fieldOfView = originalCamera.fieldOfView;
            camera.transform.SetPositionAndRotation(new Vector3(0, originalCamera.transform.position.y, originalCamera.transform.position.z - origin.z), originalCamera.transform.rotation);
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.055f,.07f,.1f);
            hero.GetComponent<PlayerMotor>().Configure(hero.GetComponent<MoveInput>(), player, camera);
            hero.GetComponent<AutoAim>().Configure(registry, hero.GetComponent<HitscanWeapon>());
            var tracer = new GameObject("Shot trace").AddComponent<LineRenderer>();
            tracer.positionCount = 2; tracer.startWidth = tracer.endWidth = .035f;
            tracer.sharedMaterial = Mat("CombatLab/Tracer"); tracer.enabled = false;
            hero.GetComponent<ShotTracer>().Configure(hero.GetComponent<HitscanWeapon>(), tracer);
            var light = new GameObject("Directional Light").AddComponent<Light>(); light.type = LightType.Directional;
            light.intensity = 1.2f; light.transform.rotation = Quaternion.Euler(50,330,0);
            RenderSettings.ambientLight = new Color(.55f,.6f,.7f);
            var track = new GameObject("Track visuals - no physics").transform;
            var station = new GameObject("Station visuals - both platforms - no physics").transform;
            var scenery = new GameObject("Journey scenery - no physics").transform;
            var fixedTrain = new GameObject("Train and station - fixed gameplay navigation");
            var wagons = new WagonRuntime[count];
            if (!AssetDatabase.IsValidFolder(NavFolder)) AssetDatabase.CreateFolder("Assets/Game/Content", "IntegrationNavigation");
            for (int i = 0; i < count; i++)
            {
                var root = new GameObject("wagon-" + (char)('a'+i)); root.transform.SetParent(fixedTrain.transform,false);root.transform.localPosition = Vector3.right * (7.41f*i);
                var visuals = Object.Instantiate(source.gameObject); SceneManager.MoveGameObjectToScene(visuals, scene);
                if (PrefabUtility.IsPartOfPrefabInstance(visuals)) PrefabUtility.UnpackPrefabInstance(visuals, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
                visuals.name = "Original prototype wagon visuals";
                visuals.transform.position -= origin; visuals.transform.SetParent(root.transform, false);
                foreach (var script in visuals.GetComponentsInChildren<MonoBehaviour>(true)) Object.DestroyImmediate(script);
                foreach (var collider in visuals.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(collider);
                foreach (var renderer in visuals.GetComponentsInChildren<Renderer>())
                    if (renderer.transform.parent.name == "DOORS") renderer.enabled = false;
                var geometryRoot = new GameObject("Fixed gameplay geometry"); geometryRoot.transform.SetParent(root.transform,false);
                var physics = geometryRoot.transform;
                float halfLength = sourceBody.extents.x;
                Cube("Floor",physics,new Vector3(0,-.1f,0),new Vector3(halfLength*2,.2f,2.54f),null,true);
                Cube("End wall A",physics,new Vector3(-halfLength+.08f,.8f,0),new Vector3(.16f,1.6f,2.54f),null,true);
                Cube("End wall B",physics,new Vector3(halfLength-.08f,.8f,0),new Vector3(.16f,1.6f,2.54f),null,true);
                var doors = new DoorController[6]; var spawns = new Transform[12];
                for (int side = 0; side < 2; side++)
                {
                    float sign = side == 0 ? -1 : 1;
                    var sideBounds = doorBounds.Where(b => Mathf.Sign(b.center.z-origin.z)==sign).OrderBy(b=>b.center.x).ToArray();
                    float previous = -halfLength;
                    for (int j = 0; j < 3; j++)
                    {
                        Bounds b = sideBounds[j]; float x = b.center.x-origin.x, width = b.size.x;
                        Wall(previous,x-width*.5f); previous=x+width*.5f;
                        int index=j*2+side;
                        var entry = new GameObject($"Door {index+1} {(side==0?"south":"north")}"); entry.transform.SetParent(physics,false);
                        entry.transform.localPosition = new Vector3(x,0,sign*1.27f);
                        entry.transform.localRotation = Quaternion.Euler(0,side==0?0:180,0);
                        var block = Cube("Movement blocker",entry.transform,new Vector3(0,1,0),new Vector3(width,2,.12f),null,true);
                        block.layer=LayerMask.NameToLayer("ShotTransparent"); block.AddComponent<NavMeshModifier>().ignoreFromBuild=true;
                        var cut=block.AddComponent<NavMeshObstacle>();cut.shape=NavMeshObstacleShape.Box;cut.size=Vector3.one;cut.carving=true;cut.carveOnlyStationary=false;
                        var outside=Anchor("Outside approach",entry.transform,new Vector3(0,0,-.75f));
                        var inside=Anchor("Inside destination",entry.transform,new Vector3(0,0,1.27f));
                        var portal=entry.AddComponent<EntryPortal>();portal.Configure(cut,block.GetComponent<BoxCollider>(),outside,inside);
                        var health=entry.AddComponent<HealthComponent>(); SetHealth(health,10*(i+1),Team.Neutral);
                        var repair=Anchor("Repair anchor",entry.transform,new Vector3(0,0,.65f));
                        var door=entry.AddComponent<DoorController>();door.Configure(health,portal,repair);doors[index]=door;
                        var solid=Cube("Lower panel",entry.transform,new Vector3(0,.3f,0),new Vector3(width,.6f,.12f),Mat("NavigationLab/Door"),true);
                        solid.AddComponent<NavMeshModifier>().ignoreFromBuild=true;
                        var glass=Cube("Shoot-through glass",entry.transform,new Vector3(0,.96f,0),new Vector3(width,.72f,.12f),Mat("CombatLab/DoorWindow"),false);
                        entry.AddComponent<DoorPanels>().Configure(portal,solid,glass);
                        Cube("Repair marker",entry.transform,new Vector3(0,.01f,.65f),new Vector3(.45f,.02f,.45f),Mat("NavigationLab/Marker"),false);
                        Cube("Sill",physics,new Vector3(x,-.1f,sign*1.31f),new Vector3(width,.2f,.2f),null,true);
                        for (int row=0;row<2;row++) spawns[index+row*6]=Anchor("Station spawn "+(index+row*6),root.transform,new Vector3(x,0,sign*(4.5f+row)));
                    }
                    Wall(previous,halfLength);
                    for(int k=-2;k<=2;k++)
                    { Cube("Station column",station,new Vector3(i*7.41f+k*1.4f,.65f,sign*6.5f),new Vector3(.15f,1.3f,.15f),Mat("NavigationLab/Wall"),false); }
                    void Wall(float a,float b) { if(b>a) Cube("Wall segment",physics,new Vector3((a+b)*.5f,.8f,sign*1.27f),new Vector3(b-a,1.6f,.12f),null,true); }
                }
                var area=root.AddComponent<MovementArea>(); area.Configure(new Vector2(-halfLength+.16f,-1.2f),new Vector2(halfLength-.16f,1.2f));
                var safe=new Transform[25]; int n=0;
                safe[n++]=Anchor("Safe center",root.transform,new Vector3(0,.05f,0));
                for(int x=0;x<8;x++)for(int z=0;z<3;z++)safe[n++]=Anchor("Safe "+n,root.transform,new Vector3(-2.45f+x*.7f,.05f,-.65f+z*.65f));
                var space=root.AddComponent<WagonGeometry>();space.Configure(new Bounds(new Vector3(0,1,0),new Vector3(halfLength*2-.32f,3,2.54f)),doors,spawns,safe);
                var pool=new GameObject("Enemy pool capacity 12").AddComponent<EnemyPool>();pool.transform.SetParent(root.transform,false);
                pool.Configure(template.GetComponent<EnemyBrain>(),doors[0],player,registry);
                var wagon=root.AddComponent<WagonRuntime>();wagon.Configure(root.name,doors,pool,area);wagon.ConfigureGeometry(space);wagons[i]=wagon;
            }
            for(int side=-1;side<=1;side+=2)
            {
                Vector3 position=new Vector3((count-1)*3.705f,-.1f,side*4.335f),size=new Vector3(count*7.41f,.2f,6);
                Cube("Continuous fixed platform "+side,fixedTrain.transform,position,size,null,true);
                Cube("Continuous moving platform "+side,station,position,size,Mat("NavigationLab/Station"),false);
            }
            var surface=fixedTrain.AddComponent<NavMeshSurface>();surface.collectObjects=CollectObjects.Children;surface.useGeometry=NavMeshCollectGeometry.PhysicsColliders;
            surface.agentTypeID=agentType;surface.overrideVoxelSize=true;surface.voxelSize=.04f;
            // One continuous station topology; wagon ownership is explicit data, never an artificial platform gap.
            surface.BuildNavMesh();surface.navMeshData.name=$"Train-{count}";string navPath=$"{NavFolder}/Train-{count}.asset";
            var existing=AssetDatabase.LoadAssetAtPath<NavMeshData>(navPath);
            if(existing==null)AssetDatabase.CreateAsset(surface.navMeshData,navPath);
            else{EditorUtility.CopySerialized(surface.navMeshData,existing);surface.navMeshData=existing;EditorUtility.SetDirty(existing);}
            EditorSceneManager.CloseScene(reference,true);EditorSceneManager.CloseScene(lab,true);
            var run=new GameObject("Run flow - existing authority").AddComponent<RunDriver>();
            hero.GetComponent<ProximityRepair>().Configure(wagons[0].Doors[0],player);
            hero.GetComponent<PlayerMotor>().SetMovementArea(wagons[0].Area);
            hero.transform.position=new Vector3(0,.05f,0);
            run.Configure(wagons,player,hero.GetComponent<PlayerMotor>(),hero.GetComponent<MoveInput>(),hero.GetComponent<AutoAim>(),hero.GetComponent<HitscanWeapon>(),hero.GetComponent<ProximityRepair>());
            var spawner=new GameObject("Bounded station spawner").AddComponent<StationSpawner>();spawner.Configure(run);
            var presentation=new GameObject("Journey presentation").AddComponent<RunPresentation>();presentation.Configure(run,track,camera);presentation.ConfigureJourney(station,scenery);presentation.ConfigurePlayerFollow(true);
            for(int x=-14;x<count*8+16;x+=2) Cube("Sleeper",track,new Vector3(x,-.22f,0),new Vector3(.15f,.1f,2.1f),Mat("NavigationLab/Wall"),false);
            for(int s=-1;s<=1;s+=2)
            { Cube("Rail",track,new Vector3((count-1)*3.705f,-.17f,s*.85f),new Vector3(count*7.41f+30,.08f,.08f),Mat("NavigationLab/Wall"),false);
              for(int x=-20;x<count*8+40;x+=4)
              { Cube("Journey ground",scenery,new Vector3(x,-.35f,s*8),new Vector3(4,.2f,10),Mat("NavigationLab/Station"),false);
                Cube("Scenery marker",scenery,new Vector3(x,-.15f,s*9),new Vector3(1.5f,.15f,2),Mat("NavigationLab/Wagon"),false); } }
            new GameObject("Integration HUD and checks").AddComponent<TrainIntegrationController>().Configure(run,spawner,presentation);
            EditorSceneManager.SaveScene(scene,ScenePath);AssetDatabase.SaveAssets();Selection.activeGameObject=hero;
            Debug.Log($"[TrainIntegration] Built {count} original-prototype wagons, {count*6} doors, bilateral spawns and freshly baked geometry.");
        }
        private static Material Mat(string path)=>AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Content/"+path+".mat");
        private static int EnsureAgentType()
        {
            for(int i=0;i<NavMesh.GetSettingsCount();i++)
            { int id=NavMesh.GetSettingsByIndex(i).agentTypeID;if(NavMesh.GetSettingsNameFromID(id)=="Train Zombie")return id; }
            int created=NavMesh.CreateSettings().agentTypeID;
            var project=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/NavMeshAreas.asset")[0]);
            var settings=project.FindProperty("m_Settings");var names=project.FindProperty("m_SettingNames");
            for(int i=0;i<settings.arraySize;i++) if(settings.GetArrayElementAtIndex(i).FindPropertyRelative("agentTypeID").intValue==created)
            {
                var item=settings.GetArrayElementAtIndex(i);item.FindPropertyRelative("agentRadius").floatValue=.3f;
                item.FindPropertyRelative("agentHeight").floatValue=1.7f;item.FindPropertyRelative("agentClimb").floatValue=.2f;
                names.GetArrayElementAtIndex(i).stringValue="Train Zombie";
            }
            project.ApplyModifiedPropertiesWithoutUndo();AssetDatabase.SaveAssets();return created;
        }
        private static Transform Anchor(string name,Transform parent,Vector3 local)
        { var a=new GameObject(name).transform;a.SetParent(parent,false);a.localPosition=local;return a; }
        private static GameObject Cube(string name,Transform parent,Vector3 local,Vector3 size,Material material,bool collider)
        {
            var cube=GameObject.CreatePrimitive(PrimitiveType.Cube);cube.name=name;cube.transform.SetParent(parent,false);
            cube.transform.localPosition=local;cube.transform.localScale=size;
            if(material!=null)cube.GetComponent<Renderer>().sharedMaterial=material;else cube.GetComponent<Renderer>().enabled=false;
            if(!collider)Object.DestroyImmediate(cube.GetComponent<Collider>());return cube;
        }
        private static void SetHealth(HealthComponent health,float max,Team team)
        { var s=new SerializedObject(health);s.FindProperty("startingMaxHealth").floatValue=max;s.FindProperty("team").intValue=(int)team;s.ApplyModifiedPropertiesWithoutUndo(); }
    }
}
