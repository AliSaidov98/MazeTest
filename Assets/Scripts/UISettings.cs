using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UISettings : MonoBehaviour
{
    [SerializeField] private InputField[] _input;
    [SerializeField] private MazeGen _maze;
    [SerializeField] private Camera _cam;

    private int _width;
    private int _height;
    
    /*public void MakeOnlyEven(Slider slider)
    {
        if (Math.Abs(slider.value % 2) > 0)
            slider.value--;
    }*/

    public void GenerateMaze()
    {
        _width = Convert.ToInt32(_input[1].text);
        _height = Convert.ToInt32(_input[0].text);
        
        CheckInput();

        //Set numbers of row and cols to text
        _input[1].text = _width.ToString();
        _input[0].text = _height.ToString();
        
        //Set camera orthographic size as the whole maze can be seen
        if(_width > _height)
            _cam.DOOrthoSize(_width * 0.215f,0.3f);
        else if(_width < _height)
            _cam.DOOrthoSize(_height * 0.205f,0.3f);
        else
            _cam.DOOrthoSize(Mathf.Sqrt(_width * _width + _height * _height) * 0.155f,0.3f);
        
        //Generate the Maze
        _maze.GenerateMaze(_width, _height);
    }

    //Check if the values are even numbers and more than 4
    private void CheckInput()
    {
        if (IsOdd(_width)) _width--;
        if (IsOdd(_height)) _height--;

        if (_width <= 3) _width = 4;
        if (_height <= 3) _height = 4;
    }
    
    private bool IsOdd(int value)
    {
        return value % 2 != 0;
    }
}
