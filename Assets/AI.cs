using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using Panda;

public class AI : MonoBehaviour
{
    //Posição do Player
    public Transform player;
    //Ponto de Spawn da Bala
    public Transform bulletSpawn;
    //Slider da vida do personagem
    public Slider healthBar;   
    //Prefab pra instanciar a bala
    public GameObject bulletPrefab;

    NavMeshAgent agent;
    public Vector3 destination; // The movement destination.
    public Vector3 target;      // The position to aim to.
    float health = 100.0f;
    float rotSpeed = 5.0f;

    //Campo de visão
    float visibleRange = 80.0f;
    //Distancia do tiro
    float shotRange = 40.0f;

    void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();
        agent.stoppingDistance = shotRange - 5; //for a little buffer
        //Atualização da vida depois de um tempo para recuperar a vida
        InvokeRepeating("UpdateHealth",5,0.5f);
    }

    void Update()
    {
        //Posição da barra de vida
        Vector3 healthBarPos = Camera.main.WorldToScreenPoint(this.transform.position);
        //Valor da vida
        healthBar.value = (int)health;
        //Offset da barra
        healthBar.transform.position = healthBarPos + new Vector3(0,60,0);
    }

    void UpdateHealth()
    {
       if(health < 100)
        health ++;
    }

    void OnCollisionEnter(Collision col)
    {
        //Colisão do tiro para diminuir a vida.
        if(col.gameObject.tag == "bullet")
        {
            health -= 10;
        }
    }

    //Árvore pro Wander
    [Task] 
    public void PickRandomDestination() { 
        //Vetor 3 aleatório de posição
        Vector3 dest = new Vector3(Random.Range(-100, 100), 0, Random.Range(-100, 100)); 
        //NavMesh para posição aleatória
        agent.SetDestination(dest); 
        //Panda - Succeed
        Task.current.Succeed(); 
    }

    [Task] 
    public void MoveToDestination() { 
        //Debug para a movimentação através do tempo
        if (Task.isInspected)
            Task.current.debugInfo = string.Format("t={0:0.00}", Time.time); 
        //Verificação de chegada no ponto
        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending) { 
            Task.current.Succeed(); 
        } 
    }
    //Seleciona um destino pré definido.
    [Task] 
    public void PickDestination(int x, int z) { 
        Vector3 dest = new Vector3(x, 0, z); 
        agent.SetDestination(dest); 
        Task.current.Succeed();
    }
    //Transforma Target para Player
    [Task]
    public void TargetPlayer() { 
        target = player.transform.position; 
        Task.current.Succeed(); 
    }
    //Função de tiro
    [Task]
    public bool Fire() {
        GameObject bullet = GameObject.Instantiate(bulletPrefab, bulletSpawn.transform.position, bulletSpawn.transform.rotation); 
        bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * 2000);
        return true;
     }

    //Prende a mira no personagem.
    [Task] 
    public void LookAtTarget() { 
        Vector3 direction = target - this.transform.position;
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * rotSpeed);
        if (Task.isInspected) 
            Task.current.debugInfo = string.Format("angle={0}", Vector3.Angle(this.transform.forward, direction)); 
        if (Vector3.Angle(this.transform.forward, direction) < 5.0f) { 
            Task.current.Succeed(); 
        } 
    }

    //Condição de raycast para ver se enxerga o player ou parede
    [Task]
    bool SeePlayer()
    {
        Vector3 distance = player.transform.position - this.transform.position;
        RaycastHit hit;
        bool seeWall = false;
        Debug.DrawRay(this.transform.position, distance, Color.red);
        if (Physics.Raycast(this.transform.position, distance, out hit))
        {
            if (hit.collider.gameObject.tag == "wall")
            {
                seeWall = true;
            }
        }
        if (Task.isInspected)
            Task.current.debugInfo = string.Format("wall={0}", seeWall);
        if (distance.magnitude < visibleRange && !seeWall)
            return true;
        else
            return false;
    }

    //Função pra mudar ângulo para posição
    [Task] 
    bool Turn(float angle) {
        var p = this.transform.position + Quaternion.AngleAxis(angle, Vector3.up) * this.transform.forward;
        target = p; 
        return true; 
    }
}

