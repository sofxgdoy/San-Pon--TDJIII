using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogoManager : MonoBehaviour
{
    public Text textoDialogo;
    
    
    //variable que trackeara las oraciones

    private Queue<string> oraciones;  //lista de oraciones
    
    void Start()
    {
        oraciones= new Queue<string>();
    }

   public void ArrancarDialogo (Dialogo dialogo) {
                    

       oraciones.Clear();     

       foreach (string oracion in dialogo.oraciones) {
           oraciones.Enqueue(oracion);
        }

       MostrarSiguienteOracion();
   }

   public void MostrarSiguienteOracion() {
       if(oraciones.Count == 0) {
           FinalizarDialogo();
           return;
       }

       string oracion = oraciones.Dequeue();
       textoDialogo.text = oracion;
   }

   void FinalizarDialogo() {
       Debug.Log("FinalizarDialogo!");
       //DesactivarBoton();
   }

   //COnsultar en clase ¿Como borro de la pantalla el botón de 
   //continuar una vez que terminan los dialogos?

   /*public DesactivarBoton() {
       /Continuar botonc = gameObject.GetComponent<Continuar>();
       Continuar.SetActive(false);
       Debug.Log("DesactivarBoton!");
   }*/
}
