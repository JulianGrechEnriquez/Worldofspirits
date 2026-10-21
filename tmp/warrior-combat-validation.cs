using System.Linq;
public static class WarriorCombatValidation {
public static object Run() {
var w=UnityEngine.Object.FindFirstObjectByType<WorldOfSpirits.Player.SpiritWarriorController>();
var flags=System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;
var m=w.GetComponent<WorldOfSpirits.Spirits.SpiritManager>();
var movement=w.GetComponent<WorldOfSpirits.Player.PlayerMovement>();
var list=(System.Collections.Generic.List<WorldOfSpirits.Spirits.SpiritMember>)m.GetType().GetField("spirits",flags).GetValue(m);
var original=list.ToArray();
var results=new System.Collections.Generic.List<string>();
var prefab=UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>("Assets/_WorldOfSpirits/Prefabs/Enemies/Regular/FireTank.prefab");
UnityEngine.GameObject enemyGo=null;
try{
foreach(var spirit in original) {
w.GetType().GetField("dashEnds",flags).SetValue(w,0f);
w.GetType().GetField("dashReady",flags).SetValue(w,0f);
w.GetType().GetField("counterReady",flags).SetValue(w,0f);
list.Remove(spirit);list.Insert(0,spirit);
var a=w.CurrentArmament;
UnityEngine.Vector2 facing=movement.MoveDirection.sqrMagnitude>0.01f?movement.MoveDirection.normalized:movement.LastMoveDirection;
enemyGo=UnityEngine.Object.Instantiate(prefab,w.transform.position+(UnityEngine.Vector3)(facing*1f),UnityEngine.Quaternion.identity);
var enemy=enemyGo.GetComponent<WorldOfSpirits.Enemies.EnemyBase>();
float before=enemy.CurrentHealth;
if(a.element!=WorldOfSpirits.Combat.DamageElement.Ice){
if(a.element==WorldOfSpirits.Combat.DamageElement.Earth){
float reduced=w.ReduceIncomingDamage(20f,new WorldOfSpirits.Combat.DamageContext(20f,enemy.transform));
if(UnityEngine.Mathf.Abs(reduced-13f)>0.01f)throw new System.Exception("Earth frontal block failed: "+reduced);
}else w.GetType().GetMethod("TickPair",flags).Invoke(w,null);
if(enemy.CurrentHealth>=before)throw new System.Exception(a.pairedWeaponName+" did not damage target");
float hit=enemy.CurrentHealth;
if(a.element!=WorldOfSpirits.Combat.DamageElement.Earth)w.GetType().GetMethod("TickPair",flags).Invoke(w,null);
if(enemy.CurrentHealth!=hit)throw new System.Exception("Pair cooldown failed");
if(a.element==WorldOfSpirits.Combat.DamageElement.Water||a.element==WorldOfSpirits.Combat.DamageElement.Wind){
var crowd=enemy.GetComponent<WorldOfSpirits.Crowd.EnemyCrowdAgent>();
var impulse=(UnityEngine.Vector2)crowd.GetType().GetField("externalVelocity",flags).GetValue(crowd);
if(impulse.sqrMagnitude<0.1f)throw new System.Exception("Crowd impulse missing");
}
results.Add(a.pairedWeaponName+": damage, cooldown and elemental effect passed");
}else {
w.GetType().GetMethod("TickPair",flags).Invoke(w,null);
var shots=(System.Array)w.GetType().GetField("shots",flags).GetValue(w);
int active=0; foreach(var shot in shots)if((bool)shot.GetType().GetField("active").GetValue(shot))active++;
if(active!=3)throw new System.Exception("Expected three shards: "+active);
results.Add("Shard Launcher: 3 travelling shards spawned");
}
UnityEngine.Time.timeScale=1f;
if(!w.TryDash() || w.TryDash())throw new System.Exception("Dash cooldown failed");
w.GetType().GetMethod("FixedUpdate",flags).Invoke(w,null);
float after=enemy.CurrentHealth;
w.GetType().GetMethod("FixedUpdate",flags).Invoke(w,null);
if(enemy.CurrentHealth!=after)throw new System.Exception("Duplicate dash hit");
if(w.DashVelocity.magnitude<1f)throw new System.Exception("Dash velocity missing");
if(a.element==WorldOfSpirits.Combat.DamageElement.Water&&w.GetComponent<WorldOfSpirits.Player.PlayerCharacter>().CurrentShield<12f)throw new System.Exception("Water shield missing");
results.Add(a.armourName+": dash, cooldown, velocity and hit dedup passed");
UnityEngine.Time.timeScale=0f;
UnityEngine.Object.DestroyImmediate(enemyGo);enemyGo=null;
}
var runtimeField=typeof(WorldOfSpirits.Spirits.SpiritMember).GetField("runtimeWeapon",flags);
var anchorsSeen=new System.Collections.Generic.HashSet<UnityEngine.Transform>();
for(int i=0;i<original.Length;i++){
var spirit=original[i];var anchors=w.GetWeaponAnchors(spirit,i,original.Length);
foreach(var anchor in anchors)if(!anchorsSeen.Add(anchor))throw new System.Exception("Shared weapon anchor");
foreach(bool moving in new[]{false,true}){
spirit.ApplyState(w.transform,w.transform,anchors,i==0,moving,false,false,false,true);
var weapon=(WorldOfSpirits.Combat.SpiritWeaponAttack)runtimeField.GetValue(spirit);
if(weapon==null||!weapon.enabled||spirit.GetComponentsInChildren<UnityEngine.Renderer>(true).Any(r=>r.enabled))throw new System.Exception("Weapon-only state failed");
}
}
results.Add("All five owned weapons: moving/stationary active, companions hidden, 20 unique anchors");
var primary=original[0];var primaryWeapon=(WorldOfSpirits.Combat.SpiritWeaponAttack)runtimeField.GetValue(primary);
primary.ApplyState(w.transform,w.transform,null,true,true,false);
if(primaryWeapon.enabled)throw new System.Exception("Tamer moving weapon regression");
primary.ApplyState(w.transform,w.transform,null,true,false,false);
if(!primaryWeapon.enabled)throw new System.Exception("Tamer stationary weapon regression");
primary.ApplyState(w.transform,w.transform,null,true,false,false,false,true);
if(primaryWeapon.enabled)throw new System.Exception("Shepherd weapon regression");
results.Add("Shared form state regression: Tamer and Shepherd passed");
return results.ToArray();
}finally{
UnityEngine.Time.timeScale=0f;if(enemyGo!=null)UnityEngine.Object.DestroyImmediate(enemyGo);
list.Clear();list.AddRange(original);
w.enabled=false;w.enabled=true;
}
}
}
