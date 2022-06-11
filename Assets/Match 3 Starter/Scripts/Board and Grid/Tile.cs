/*
 * Copyright (c) 2017 Razeware LLC
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tile : MonoBehaviour {
	private static Color selectedColor = new Color(.5f, .5f, .5f, 1.0f);
	private static Tile previousSelected = null;

	private SpriteRenderer render;
	private bool isSelected = false;

	private Vector2[] adjacentDirections = new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

	private bool matchFound = false;  //cuando se encuentre un match, esta variable se vuelve true

	void Awake() {
		render = GetComponent<SpriteRenderer>();
    }

	private void Select() {
		isSelected = true;
		render.color = selectedColor;
		previousSelected = gameObject.GetComponent<Tile>();
		SFXManager.instance.PlaySFX(Clip.Select);
	}

	private void Deselect() {
		isSelected = false;
		render.color = Color.white;
		previousSelected = null;
	}

    void OnMouseDown() {
	    // debemos asegurarnos que el jugador pueda seleccionar solo cuando se requiera
        if (render.sprite == null || BoardManager.instance.IsShifting) {
         return;
        }


       if (isSelected) { // 2 ya selecciono? si es asi, deseleccionar
        Deselect();
        } else {
            if (previousSelected == null) { //  es el primer tile que se selecciona? 
            Select();

            } else {
                if (GetAllAdjacentTiles().Contains(previousSelected.gameObject)) { // llama a GetAllAdjacentTiles y verifica si el previousSelected game object esta en la lista de tiles adyacentes
                    SwapSprite(previousSelected.render); // cambia el sprite
                    
					previousSelected.ClearAllMatches();
					previousSelected.Deselect();
					ClearAllMatches();
                } else { // si el tile no esta al lado del tile seleccionado anteriormente, deseleccionar el anterior y seleccionar el nuevo tile
                    previousSelected.GetComponent<Tile>().Deselect();
                    Select();
                }
            }
        }
    }

	private void SwapSprite(SpriteRenderer render2) { // acepta como parametro un spriterenderer llamado rende2, que va a ser usado con render para cambiar los sprites
        
		if (render.sprite == render2.sprite) { // compara si el sprite es el mismo por el que se quiere cambiar, no hace nada
          return;
		
		}

        Sprite tempSprite = render2.sprite; // crea un sprite temporal que contendra el sprite de render2
        render2.sprite = render.sprite; // cambia el sprite2, seteandolo como el 1
        render.sprite = tempSprite; // al sprite1, lo setea como sprite2, guardandolo en spritetemporal
        SFXManager.instance.PlaySFX(Clip.Swap); // sonido
    }

	private GameObject GetAdjacent(Vector2 castDir) {       //esta funcion recupera los tiles adyacentes enviando un raycast, si se encuentra un tile en esa direccion, se devuelve su gameObject
        RaycastHit2D hit = Physics2D.Raycast(transform.position, castDir);
        
		if (hit.collider != null) {
			return hit.collider.gameObject;
        }
        return null;
    }

	private List<GameObject> GetAllAdjacentTiles() {    //esta funcion usa GetAdjacent, para generar una lista de los tiles que rodean al tile actual
        List<GameObject> adjacentTiles = new List<GameObject>();    //loopea en todas direcciones y duelve los tiles adyacentes a la lista adjacentTiles
        
		for (int i = 0; i < adjacentDirections.Length; i++) {        //con esta funcion podemos forzar al tile a cambiar solo con los adyacentes.
            adjacentTiles.Add(GetAdjacent(adjacentDirections[i]));
        }
        return adjacentTiles;
    }

	private List<GameObject> FindMatch(Vector2 castDir) { // esta funcion acepta un vector2 como parametro, que va a ser la direccion en la que va a ir cada raycast
        List<GameObject> matchingTiles = new List<GameObject>(); // crea una nueva lista que contendra los matching tiles
        RaycastHit2D hit = Physics2D.Raycast(transform.position, castDir); // manda un raycast a la direccion de castDir
          
		while (hit.collider != null && hit.collider.GetComponent<SpriteRenderer>().sprite == render.sprite) { // continua mandando raycast hasta que no golpeen nada o el tile sprite sea diferente al del gameobject. En ambos casos, lo considera match y se aniade a la lista.
            matchingTiles.Add(hit.collider.gameObject);
            hit = Physics2D.Raycast(hit.collider.transform.position, castDir);

        }
        return matchingTiles; // devuelve la lista de matching sprites
    }

	//esto encuentra todas las matching tiles y los limpia ->
	private void ClearMatch(Vector2[] paths) // toma un array vector2 como path, y en estos el tile va a enviar los raycast
	{
        List<GameObject> matchingTiles = new List<GameObject>(); // crea una lista para guardar los match
        for (int i = 0; i < paths.Length; i++) // hace una iteracion por cada path y aniade a la lista los matching tiles
        {
            matchingTiles.AddRange(FindMatch(paths[i]));
        }

        if (matchingTiles.Count >= 2) // continua si se encuentra un match de dos o mas
        {
            StartCoroutine(esperarYCargar());
            for (int i = 0; i < matchingTiles.Count; i++) // hace una iteracion por cada matching tile y remueve sus sprites seteandolos en null
            {
             StartCoroutine(esperarYCargar());
            matchingTiles[i].GetComponent<SpriteRenderer>().sprite = null;
             StartCoroutine(esperarYCargar());
            
            }
            matchFound = true; // setea matchFound a true :D 
        }
        
    }

	public void ClearAllMatches() {
        if (render.sprite == null)
        return;

        StartCoroutine(esperarYCargar());

        ClearMatch(new Vector2[2] { Vector2.left, Vector2.right });
        ClearMatch(new Vector2[2] { Vector2.up, Vector2.down });
        
		if (matchFound) {
            render.sprite = null;
            matchFound = false;
            StopCoroutine(BoardManager.instance.FindNullTiles());   //estas dos lineas detienen FindNullTiles y le dicen que vuelva a arrancar desde el principio
            StartCoroutine(BoardManager.instance.FindNullTiles());
            SFXManager.instance.PlaySFX(Clip.Clear);
            GUIManager.instance.MoveCounter--; //esto decrece el contador cada vez que el sprite se cambia
        }
    }

    private IEnumerator esperarYCargar() {
        yield return new WaitForSecondsRealtime(1f);
    }

}