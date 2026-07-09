//
//
//NOTES:
//
//This script is used for DEMONSTRATION porpuses of the Projectiles. I recommend everyone to create their own code for their own projects.
//THIS IS JUST A BASIC EXAMPLE PUT TOGETHER TO DEMONSTRATE VFX ASSETS.
//
//




#pragma warning disable 0168 // variable declared but not used.
#pragma warning disable 0219 // variable assigned but not used.
#pragma warning disable 0414 // private field assigned but not used.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileMoveScript : MonoBehaviour {

    public bool rotate = false;
    public float rotateAmount = 45;
    public bool bounce = false;
    public float bounceForce = 10;
    public float speed;
	[Tooltip("From 0% to 100%")]
	public float accuracy;
	public float fireRate;
	public GameObject muzzlePrefab;
	public GameObject hitPrefab;
	public List<GameObject> trails;

    private Vector3 startPos;
	private float speedRandomness;
	private Vector3 offset;
	private bool collided;
	private Rigidbody rb;
    private RotateToMouseScript rotateToMouse;
    private GameObject target;

	void Start () {
        startPos = transform.position;
        rb = GetComponent <Rigidbody> ();

		//used to create a radius for the accuracy and have a very unique randomness
		if (accuracy != 100) {
			accuracy = 1 - (accuracy / 100);

			for (int i = 0; i < 2; i++) {
				var val = 1 * Random.Range (-accuracy, accuracy);
				var index = Random.Range (0, 2);
				if (i == 0) {
					if (index == 0)
						offset = new Vector3 (0, -val, 0);
					else
						offset = new Vector3 (0, val, 0);
				} else {
					if (index == 0)
						offset = new Vector3 (0, offset.y, -val);
					else
						offset = new Vector3 (0, offset.y, val);
				}
			}
		}
			
		if (muzzlePrefab != null) {
			var muzzleVFX = Instantiate (muzzlePrefab, transform.position, Quaternion.identity);
			muzzleVFX.transform.forward = gameObject.transform.forward + offset;
			var ps = muzzleVFX.GetComponent<ParticleSystem>();
			if (ps != null)
				Destroy (muzzleVFX, ps.main.duration);
			else {
				var psChild = muzzleVFX.transform.GetChild(0).GetComponent<ParticleSystem>();
				Destroy (muzzleVFX, psChild.main.duration);
			}
		}
	}

	void FixedUpdate () {
        if (target != null)
            rotateToMouse.RotateToMouse (gameObject, target.transform.position);
        if (rotate)
            transform.Rotate(0, 0, rotateAmount, Space.Self);
        if (speed != 0 && rb != null)
			rb.position += (transform.forward + offset) * (speed * Time.deltaTime);   
    }

    //void OnCollisionEnter (Collision co) {
    //       if (!bounce)
    //       {
    //           if (co.gameObject.tag != "Bullet" && !collided)
    //           {
    //               collided = true;

    //               if (trails.Count > 0)
    //               {
    //                   for (int i = 0; i < trails.Count; i++)
    //                   {
    //                       trails[i].transform.parent = null;
    //                       var _ps = trails[i].GetComponent<ParticleSystem>();
    //                       if (_ps != null)
    //                       {
    //                           _ps.Stop();
    //                           Destroy(_ps.gameObject, _ps.main.duration + _ps.main.startLifetime.constantMax);
    //                       }
    //                   }
    //               }

    //               speed = 0;
    //               GetComponent<Rigidbody>().isKinematic = true;

    //               ContactPoint contact = co.contacts[0];
    //               Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
    //               Vector3 pos = contact.point;

    //               if (hitPrefab != null)
    //               {
    //                   var hitVFX = Instantiate(hitPrefab, pos, rot) as GameObject;

    //                   var _ps = hitVFX.GetComponent<ParticleSystem>();
    //                   if (_ps == null)
    //                   {
    //                       var psChild = hitVFX.transform.GetChild(0).GetComponent<ParticleSystem>();
    //                       Destroy(hitVFX, psChild.main.duration);
    //                   }
    //                   else
    //                       Destroy(hitVFX, _ps.main.duration);
    //               }

    //               StartCoroutine(DestroyParticle(0f));
    //           }
    //       }
    //       else
    //       {
    //           rb.useGravity = true;
    //           rb.drag = 0.5f;
    //           ContactPoint contact = co.contacts[0];
    //           rb.AddForce (Vector3.Reflect((contact.point - startPos).normalized, contact.normal) * bounceForce, ForceMode.Impulse);
    //           Destroy ( this );
    //       }
    //}

    void OnCollisionEnter(Collision co)
    {
        if (!bounce)
        {
            if (co.gameObject.tag != "Bullet" && !collided)
            {
                collided = true;

                gameObject.GetComponent<ParticleSystem>().Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);

                if (trails.Count > 0)
                {
                    for (int i = 0; i < trails.Count; i++)
                    {
                        if (trails[i] != null)
                        {
                            var ps = trails[i].GetComponent<ParticleSystem>();
                            if (ps != null)
                            {
                                // 停止产生新粒子，但空气中已经发射出去的老粒子会优雅留在原地继续飘散
                                ps.Stop();
                            }
                        }
                    }
                }

                // 3. 物理急刹车：速度归零，刚体转为运动学，让子弹死死钉在原地不再发生位移
                speed = 0;
                var myRigidbody = GetComponent<Rigidbody>();
                if (myRigidbody != null)
                {
                    myRigidbody.velocity = Vector3.zero; // Unity 2022+ 请用 linearVelocity，旧版用 velocity
                    myRigidbody.isKinematic = true;
                }
                // 同时关闭碰撞体，防止隐形期间被其他物理物体撞飞
                var myCollider = GetComponent<Collider>();
                if (myCollider != null) myCollider.enabled = false;

                // 4. 爆裂/击中特效生成（Hit VFX）
                ContactPoint contact = co.contacts[0];
                // 旋转方向根据你之前的偏好，可以直接继承子弹本身方向（加上你的反向修正），或者使用法线：
                Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
                Vector3 pos = contact.point;

                if (hitPrefab != null)
                {
                    var hitVFX = Instantiate(hitPrefab, pos, rot) as GameObject;
                    var ps = hitVFX.GetComponent<ParticleSystem>();
                    if (ps == null)
                    {
                        var psChild = hitVFX.transform.GetChild(0).GetComponent<ParticleSystem>();
                        Destroy(hitVFX, psChild.main.duration);
                    }
                    else
                        Destroy(hitVFX, ps.main.duration);
                }

                float maxWaitTime = CalculateMaxTrailDuration();
                StartCoroutine(DestroyParticle(maxWaitTime));
            }
        }
        else
        {
            // 弹跳子弹逻辑保持原样
            rb.useGravity = true;
            rb.drag = 0.5f;
            ContactPoint contact = co.contacts[0];
            rb.AddForce(Vector3.Reflect((contact.point - startPos).normalized, contact.normal) * bounceForce, ForceMode.Impulse);
            Destroy(this);
        }
    }

    private float CalculateMaxTrailDuration()
    {
        float maxTime = 0.5f; // 给一个基础保底等待时间
        for (int i = 0; i < trails.Count; i++)
        {
            if (trails[i] != null)
            {
                var ps = trails[i].GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    // 公式：发射持续周期 + 单颗粒子的最大存活寿命 = 该粒子系统彻底燃尽的时间
                    float t = ps.main.duration + ps.main.startLifetime.constantMax;
                    if (t > maxTime) maxTime = t;
                }
            }
        }
        return maxTime;
    }

    public IEnumerator DestroyParticle (float waitTime) {

		if (transform.childCount > 0 && waitTime != 0) {
			List<Transform> tList = new List<Transform> ();

			foreach (Transform t in transform.GetChild(0).transform) {
				tList.Add (t);
			}		

			while (transform.GetChild(0).localScale.x > 0) {
				yield return new WaitForSeconds (0.01f);
				transform.GetChild(0).localScale -= new Vector3 (0.1f, 0.1f, 0.1f);
				for (int i = 0; i < tList.Count; i++) {
					tList[i].localScale -= new Vector3 (0.1f, 0.1f, 0.1f);
				}
			}
		}
		
		yield return new WaitForSeconds (waitTime);
		Destroy (gameObject);
	}

    public void SetTarget (GameObject trg, RotateToMouseScript rotateTo)
    {
        target = trg;
        rotateToMouse = rotateTo;
    }
}
