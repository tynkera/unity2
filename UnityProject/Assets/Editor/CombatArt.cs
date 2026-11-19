using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
namespace ExpeditionCamp.EditorTools
{
    public static class CombatArt
    {
        static Material Mat(string name, Color c)
        {
            string path="Assets/Materials/Model_"+name+".mat";
            var m=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
            m.color=c;m.SetFloat("_Smoothness",.12f);return m;
        }
        static Mesh Gem()
        {
            const string path="Assets/Art/ModelFacet.asset";
            var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(mesh!=null)return mesh;
            var verts=new List<Vector3>();var tris=new List<int>();
            Vector3[] ring={Vector3.forward,Vector3.right,Vector3.back,Vector3.left};
            for(int i=0;i<4;i++)foreach(float sign in new[]{1f,-1f})
            {
                Vector3 a=Vector3.up*sign,b=ring[i],c=ring[(i+1)%4];
                if(sign<0){var t=b;b=c;c=t;}
                Vector3 ab=(a+b).normalized,bc=(b+c).normalized,ca=(c+a).normalized;
                Vector3[] vs={a,ab,ca, ab,b,bc, ca,bc,c, ab,bc,ca};
                foreach(var v in vs){tris.Add(verts.Count);verts.Add(v*.5f);}
            }
            mesh=new Mesh{name="Faceted rounded solid"};mesh.SetVertices(verts);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();AssetDatabase.CreateAsset(mesh,path);return mesh;
        }
        static Transform Part(Transform root,string name,Vector3 pos,Vector3 scale,Material mat,Vector3 rot=default)
        {
            var t=new GameObject(name).transform;t.SetParent(root,false);t.localPosition=pos;t.localScale=scale;t.localEulerAngles=rot;
            t.gameObject.AddComponent<MeshFilter>().sharedMesh=Gem();t.gameObject.AddComponent<MeshRenderer>().sharedMaterial=mat;return t;
        }
        public static Material BodyMaterial(CharacterShape shape)
        {
            switch(shape)
            {
                case CharacterShape.Crab: return Mat("Coral",new Color(.69f,.22f,.14f));
                case CharacterShape.Rabbit: return Mat("Ivory",new Color(.94f,.85f,.65f));
                case CharacterShape.Bat: return Mat("Bat",new Color(.30f,.22f,.42f));
                case CharacterShape.Golem: return Mat("Stone",new Color(.39f,.47f,.43f));
                case CharacterShape.Wolf: return Mat("Wolf",new Color(.32f,.43f,.49f));
                default: return Mat("Fox",new Color(.86f,.38f,.12f));
            }
        }
        public static Material MouseMaterial()
        {
            return Mat("Mouse",new Color(.45f,.42f,.50f));
        }
        public static HitFeedback Build(GameObject root, CharacterShape shape)
        {
            var v=new GameObject("Visual").transform;v.SetParent(root.transform,false);
            v.gameObject.layer=root.layer;
            v.gameObject.AddComponent<ModelMotion>();
            var model=v.gameObject.AddComponent<CharacterModel>();
            model.shape=shape;
            model.bodyMaterial=BodyMaterial(shape);
            EditorUtil.SetField(model,"partMesh",Gem());
            EditorUtil.SetField(model,"darkMaterial",Mat("Ink",new Color(.055f,.095f,.10f)));
            EditorUtil.SetField(model,"lightMaterial",Mat("Ivory",new Color(.94f,.85f,.65f)));
            EditorUtil.SetField(model,"clothMaterial",Mat("Cloth",new Color(.08f,.37f,.34f)));
            MouseMaterial();
            Effects(v);
            model.ClearParts();
            return v.gameObject.AddComponent<HitFeedback>();
        }
        static void Effects(Transform parent)
        {
            const string path="Assets/Materials/CombatSpark.mat";
            var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(mat==null){mat=new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));AssetDatabase.CreateAsset(mat,path);}
            mat.SetColor("_BaseColor",new Color(1.8f,1.3f,.5f,1));
            var fx=parent.gameObject.AddComponent<CombatEffects>();
            var go=new GameObject("Hit Sparks");go.transform.SetParent(parent,false);go.transform.localPosition=Vector3.up;
            var ps=go.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=ps.main;main.playOnAwake=false;main.loop=false;main.duration=.3f;main.startLifetime=new ParticleSystem.MinMaxCurve(.12f,.25f);main.startSpeed=new ParticleSystem.MinMaxCurve(2,4);main.startSize=new ParticleSystem.MinMaxCurve(.035f,.085f);main.gravityModifier=.5f;main.simulationSpace=ParticleSystemSimulationSpace.World;main.maxParticles=32;
            var emission=ps.emission;emission.enabled=false;var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Sphere;shape.radius=.15f;
            ps.GetComponent<ParticleSystemRenderer>().sharedMaterial=mat;
            var arc=new GameObject("Attack Arc");arc.transform.SetParent(parent,false);var line=arc.AddComponent<LineRenderer>();line.useWorldSpace=false;line.positionCount=12;line.widthMultiplier=.1f;line.sharedMaterial=mat;line.startColor=line.endColor=new Color(1,.86f,.46f);line.enabled=false;
            EditorUtil.SetField(fx,"sparks",ps);EditorUtil.SetField(fx,"slash",line);
        }
        public static void Drop(Transform parent)
        {
            Part(parent,"LootCrystal",Vector3.zero,new Vector3(.38f,.6f,.38f),Mat("LootGold",new Color(.95f,.66f,.18f)));
        }
        public static void Lighting(bool cave)
        {
            var cam=Camera.main;var data=cam.GetUniversalAdditionalCameraData();data.renderPostProcessing=true;data.antialiasing=AntialiasingMode.SubpixelMorphologicalAntiAliasing;cam.allowHDR=true;
            var volume=new GameObject("Global Volume — Art Direction").AddComponent<Volume>();volume.isGlobal=true;volume.priority=10;
            string path="Assets/Settings/URP/Combat"+(cave?"Cave":"Forest")+"Volume.asset";
            var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
            if(profile==null){profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,path);}
            foreach(var old in profile.components)Object.DestroyImmediate(old,true);profile.components.Clear();
            var tone=profile.Add<Tonemapping>(true);tone.mode.Override(TonemappingMode.ACES);
            var color=profile.Add<ColorAdjustments>(true);color.postExposure.Override(.25f);color.contrast.Override(12);color.saturation.Override(-8);
            var bloom=profile.Add<Bloom>(true);bloom.threshold.Override(1.15f);bloom.intensity.Override(.18f);bloom.scatter.Override(.55f);
            var vignette=profile.Add<Vignette>(true);vignette.intensity.Override(.18f);vignette.smoothness.Override(.45f);
            foreach(var c in profile.components)AssetDatabase.AddObjectToAsset(c,profile);EditorUtility.SetDirty(profile);volume.sharedProfile=profile;
            RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.64f,.73f,.79f);RenderSettings.ambientEquatorColor=new Color(.36f,.43f,.41f);RenderSettings.ambientGroundColor=new Color(.22f,.25f,.19f);
            foreach(var light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))if(light.type==LightType.Directional){light.intensity=1.65f;light.color=new Color(1,.88f,.69f);light.transform.rotation=Quaternion.Euler(48,-35,0);light.shadows=LightShadows.Soft;light.shadowBias=.035f;light.shadowNormalBias=.2f;}
            var pipe=GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;if(pipe!=null){pipe.msaaSampleCount=4;pipe.shadowDistance=55;pipe.shadowCascadeCount=2;var so=new SerializedObject(pipe);so.FindProperty("m_SoftShadowsSupported").boolValue=true;so.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(pipe);}
        }
    }
}
