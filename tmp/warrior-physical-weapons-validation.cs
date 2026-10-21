using System; using System.Linq; using System.Collections.Generic; using UnityEngine; using UnityEditor; using WorldOfSpirits.Player; using WorldOfSpirits.Spirits; using WorldOfSpirits.Enemies; using WorldOfSpirits.Crowd;
public static class WarriorPhysicalValidation {
public static object Run(){
var w=UnityEngine.Object.FindFirstObjectByType<SpiritWarriorController>();
var m=w.GetComponent<SpiritManager>();
var flags=System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;
var baseType=typeof(ThrustMeleeWeaponBase);
var list=(List<SpiritMember>)typeof(SpiritManager).GetField("spirits",flags).GetValue(m);
var original=list.ToArray();
var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_WorldOfSpirits/Prefabs/Enemies/Regular/FireTank.prefab");
GameObject target=UnityEngine.Object.Instantiate(prefab,w.transform.position+Vector3.right*2f,Quaternion.identity);
target.name="Paired Weapon Validation Target";
var enemy=target.GetComponent<EnemyBase>();enemy.IncreaseMaximumHealth(1000f);
var crowd=target.GetComponent<EnemyCrowdAgent>();
var visited=new HashSet<string>();var hands=new HashSet<string>();var results=new List<string>();
float initialHealth=enemy.CurrentHealth;float maxKnockback=0f;float maxTravel=0f;
int stage=0;double mark=EditorApplication.timeSinceStartup;
Time.timeScale=1f;
EditorApplication.CallbackFunction check=null;
check=()=>{
try{
var weapon=w.GetComponentsInChildren<WarriorPairedThrustWeapon>(true).FirstOrDefault(x=>x.enabled);
if(weapon!=null){
string phase=baseType.GetField("phase",flags).GetValue(weapon).ToString();visited.Add(phase);
var hand=(Transform)baseType.GetField("activeGauntlet",flags).GetValue(weapon);
if(hand!=null){hands.Add(hand.name);maxTravel=Mathf.Max(maxTravel,Vector3.Distance(hand.position,w.transform.position));}
}
maxKnockback=Mathf.Max(maxKnockback,((Vector2)typeof(EnemyCrowdAgent).GetField("externalVelocity",flags).GetValue(crowd)).magnitude);
double elapsed=EditorApplication.timeSinceStartup-mark;
if(stage==0&&elapsed>=1.5){
bool pass=enemy.CurrentHealth<initialHealth&&hands.Count==2&&visited.Contains("Extending")&&visited.Contains("Returning")&&maxTravel>1.5f;
results.Add((pass?"PASS":"FAIL")+": Fire talons: damage "+(initialHealth-enemy.CurrentHealth)+", alternating hands "+hands.Count+", phases "+string.Join("/",visited)+", reach "+maxTravel.ToString("0.00"));
var earth=list.First(s=>s.Definition.SpiritName.Contains("Earth"));list.Remove(earth);list.Insert(0,earth);
initialHealth=enemy.CurrentHealth;maxKnockback=0f;maxTravel=0f;visited.Clear();hands.Clear();mark=EditorApplication.timeSinceStartup;stage=1;
}else if(stage==1&&elapsed>=1.7){
bool pass=enemy.CurrentHealth<initialHealth&&maxKnockback>0.5f&&visited.Contains("Extending")&&visited.Contains("Returning")&&maxTravel>1.5f;
results.Add((pass?"PASS":"FAIL")+": Shield auto-bash: damage "+(initialHealth-enemy.CurrentHealth)+", knockback "+maxKnockback.ToString("0.0")+", phases "+string.Join("/",visited)+", reach "+maxTravel.ToString("0.00"));
Time.timeScale=0f;float health=enemy.CurrentHealth;
var shield=w.GetComponentsInChildren<WarriorPairedThrustWeapon>(true).First(x=>x.name=="Golem Shield");
list.Clear();list.AddRange(original);typeof(SpiritWarriorController).GetMethod("TickThrustWeapons",flags).Invoke(w,null);
bool hidden=!shield.enabled&&shield.GetComponentsInChildren<Renderer>(true).All(r=>!r.enabled);
results.Add((hidden?"PASS":"FAIL")+": outgoing shield hidden and disabled on main-spirit change");
System.IO.File.WriteAllLines("C:/Users/julia/OneDrive/Documents/GitHub/Worldofspirits/tmp/warrior-physical-weapons-validation.txt",results);
UnityEngine.Object.DestroyImmediate(target);EditorApplication.update-=check;
ScreenCapture.CaptureScreenshot("C:/Users/julia/OneDrive/Documents/GitHub/Worldofspirits/tmp/warrior-talons-preview.png");
}
}catch(Exception ex){Time.timeScale=0f;EditorApplication.update-=check;System.IO.File.WriteAllText("C:/Users/julia/OneDrive/Documents/GitHub/Worldofspirits/tmp/warrior-physical-weapons-validation.txt","FAIL "+ex);}
};
EditorApplication.update+=check;
return "Physical motion/damage/knockback tests scheduled";
}}
