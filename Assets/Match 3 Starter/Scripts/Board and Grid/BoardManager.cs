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

public class BoardManager : MonoBehaviour {
	public static BoardManager instance;                          //como otros scrips deben acceder al boardmanager, se crea una instancia
	public List<Sprite> characters = new List<Sprite>();     //la lista de sprites que vamos a usar como tiles para el tablero
	public GameObject tile;                                 //el prefab tile va a ser llamado desde el tablero
	public int xSize, ySize;                       //dimensiones del tablero

	private GameObject[,] tiles;           //un 2D array que va a ser usado para posicionar los tiles en el tablero

	public bool IsShifting { get; set; }     //un booleano que va a decirnos cuando se encontro un match y el tablero se esta llenando otra vez

	void Start () {
		instance = GetComponent<BoardManager>();

		Vector2 offset = tile.GetComponent<SpriteRenderer>().bounds.size;     // llama al createboard, pasando las dimensiones  del sprite de -tile-
        CreateBoard(offset.x, offset.y);
    }

	private void CreateBoard (float xOffset, float yOffset) {      
		tiles = new GameObject[xSize, ySize];                           //el array 2D tiles se inicializa 

        float startX = transform.position.x;              //encuentra la posicion  para empezar a generar el tablero
		float startY = transform.position.y;

		Sprite[] previousLeft = new Sprite[ySize];      // para que no se generen 3sprites iguales juntos de entrada, tenemos que verificar que sprites tenemos a los costados
		Sprite previousBelow = null;

		for (int x = 0; x < xSize; x++) {                //loop for, por cada iteracion genera un tile nuevo siguiendo los parametros hasta terminar el tablero
			for (int y = 0; y < ySize; y++) {
				GameObject newTile = Instantiate(tile, new Vector3(startX + (xOffset * x), startY + (yOffset * y), 0), tile.transform.rotation);
				tiles[x, y] = newTile;

				newTile.transform.parent = transform; // vuelve a los tiles parent del board para no tener problemas en el editor
                
				List<Sprite> possibleCharacters = new List<Sprite>(); // crea una lista de posibles caracteres
                possibleCharacters.AddRange(characters); // aniade todos los posibles caracteres a la lista

                possibleCharacters.Remove(previousLeft[y]); //  remueve de la lista de posibles, a los caracteres que estan a la izquierda y debajo del caracter actual
                possibleCharacters.Remove(previousBelow);

				Sprite newSprite = possibleCharacters[Random.Range(0, possibleCharacters.Count)];   // elige un sprite al azar de los que cargamos, de la lista de posibles caracteres
                newTile.GetComponent<SpriteRenderer>().sprite = newSprite;   //setea que el tile nuevo que se genera, sea el que elegimos al azar en la linea anterior

				previousLeft[y] = newSprite;
                previousBelow = newSprite;
			}
        }
    }

    
	public IEnumerator FindNullTiles() {    // esto va a interar en el tablero en busca de tiles con null sprites, y si encuentra,  llama a ShiftTilesDown para empezar a llenar
      for (int x = 0; x < xSize; x++) {
        for (int y = 0; y < ySize; y++) {
            if (tiles[x, y].GetComponent<SpriteRenderer>().sprite == null) {
                yield return StartCoroutine(ShiftTilesDown(x, y));
                break;
            }
        }
     }

	    for (int x = 0; x < xSize; x++) {
          for (int y = 0; y < ySize; y++) {
             tiles[x, y].GetComponent<Tile>().ClearAllMatches();
            }
        }
    }

	private IEnumerator ShiftTilesDown(int x, int yStart, float shiftDelay = .03f) {
      IsShifting = true;
      List<SpriteRenderer>  renders = new List<SpriteRenderer>();
      int nullCount = 0;

      for (int y = yStart; y < ySize; y++) {  // busca cuantos espacios hay que llenar hacia abajo
         SpriteRenderer render = tiles[x, y].GetComponent<SpriteRenderer>();
         if (render.sprite == null) { // almacena el valor en un integer
            nullCount++;
         }
         renders.Add(render);
        }

      for (int i = 0; i < nullCount; i++) { // loop otra vez para empezar a cambiar y llenar
         GUIManager.instance.Score += 50;
         yield return new WaitForSeconds(shiftDelay);// pausa de delay
         for (int k = 0; k < renders.Count - 1; k++) { // loop en cada sprite renderer de la lista de renders
            renders[k].sprite = renders[k + 1].sprite;
            renders[k + 1].sprite = GetNewSprite(x, ySize - 1); // aseguramos que el tablero este siempre lleno
          }
        }
      IsShifting = false;
    }

	private Sprite GetNewSprite(int x, int y) {   //crea una lista de posibles caracteres para llenar el tablero, los if aseguran que no te vayas de los parametros y q no se generen duplicados
      List<Sprite> possibleCharacters = new List<Sprite>();
      possibleCharacters.AddRange(characters);

       if (x > 0) {
          possibleCharacters.Remove(tiles[x - 1, y].GetComponent<SpriteRenderer>().sprite);
        }

        if (x < xSize - 1) {
          possibleCharacters.Remove(tiles[x + 1, y].GetComponent<SpriteRenderer>().sprite);
        }

        if (y > 0) {
        possibleCharacters.Remove(tiles[x, y - 1].GetComponent<SpriteRenderer>().sprite);
        }

       return possibleCharacters[Random.Range(0, possibleCharacters.Count)];
    }


}
