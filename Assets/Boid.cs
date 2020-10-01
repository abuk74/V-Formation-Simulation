using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    [Header("Definiowane dynamicznie")]
    public Rigidbody rigid;
    private Neighborhood neighborhood;
    //użyj tej funkcji do inicjalizacji
    void Awake()
    {
        neighborhood = GetComponent<Neighborhood>();
        rigid = GetComponent<Rigidbody>();
        //ustala losowe położenie początkowe
        pos = Random.insideUnitSphere * Spawner.S.spawnRadius;
        //ustal losową prędkość początkową
        Vector3 vel = Random.onUnitSphere * Spawner.S.velocity;
        rigid.velocity = vel;
        LookAhead();
        //zdefiniuj losowy kolor dla boida, ale upewnij się, że niej zbyt ciemny
        Color randColor = Color.black;
        while(randColor.r + randColor.g + randColor.b < 1.0f)
        {
            randColor = new Color(Random.value, Random.value, Random.value);
        }
        Renderer[] rends = gameObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in rends)
        {
            r.material.color = randColor;
        }
        TrailRenderer tRend = GetComponent<TrailRenderer>();
        tRend.material.SetColor("_TintColor", randColor);
    }
    void LookAhead()
    {
        //Obraca boida tak żeby, patrzył w kierunku w którym leci
        transform.LookAt(pos + rigid.velocity);
    }
    public Vector3 pos
    {
        get { return transform.position; }
        set { transform.position = value; }
    }
    //Odświeża się 50x na sekundę
    void FixedUpdate()
    {
        Vector3 vel = rigid.velocity;
        Spawner spn = Spawner.S;
        //Unikanie zderzenia
        Vector3 velAvoid = Vector3.zero;
        Vector3 tooClosePos = neighborhood.avgClosePos;
        //Jeśli wynik jest równy Vector3.zero nie musimy nic robić
        if (tooClosePos != Vector3.zero)
        {
            velAvoid = pos - tooClosePos;
            velAvoid.Normalize();
            velAvoid *= spn.velocity;
        }
        //Dopasowanie prędkości
        Vector3 velAlign = neighborhood.avgVel;
        //Wykonaj działania, jeśli wartość velAlign jest różna od Vector3.zero
        if (velAlign != Vector3.zero)
        {
            velAlign.Normalize();
            velAlign *= spn.velocity;
        }
        //Dążenie do środka stada
        Vector3 velCenter = neighborhood.avgPos;
        if (velCenter != Vector3.zero)
        {
            velCenter -= transform.position;
            velCenter.Normalize();
            velCenter *= spn.velocity;
        }
        //Przyciąganie- poruszanie w kieunku obiektu Attractor
        Vector3 delta = Attractor.POS - pos;
        //Sprawdzenie czy Attractor przyciąga czy odpycha
        bool attracted = (delta.magnitude > spn.attractPushDist);
        Vector3 velAttract = delta.normalized * spn.velocity;
        //Zastosuj wszystkie wartości prędkości
        float fdt = Time.fixedDeltaTime;
        if (velAvoid != Vector3.zero)
        {
            vel = Vector3.Lerp(vel, velAvoid, spn.collAvoid*fdt);
        }
        else
        {
            if (velAlign != Vector3.zero)
            {
                vel = Vector3.Lerp(vel, velAlign, spn.velMatching * fdt);
            }
            if (velCenter != Vector3.zero)
            {
                vel = Vector3.Lerp(vel, velCenter, spn.flockCentering * fdt);
            }
            if (velAttract != Vector3.zero)
            {
                if (attracted)
                {
                    vel = Vector3.Lerp(vel, velAttract, spn.attractPull * fdt);
                }
                else
                {
                    vel = Vector3.Lerp(vel, -velAttract, spn.attractPush * fdt);
                }
            }
        }
        //Przypisz zmiennej vel prędkość z singletona Spawner
        vel = vel.normalized * spn.velocity;
        //Ostatecznie przypisz ją do komponentu Rigidbody
        rigid.velocity = vel;
        LookAhead();
    }
}
