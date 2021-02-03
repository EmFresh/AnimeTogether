using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_InputField))]
public class LinkInputFieldValue : MonoBehaviour
{
    public GameObject reference;

    public string className, valName;
    PropertyInfo val;
    void Start()
    {
        var dClass = reference.GetComponent(className);
        var dType = dClass.GetType();
        var fieldValues = dClass.GetType()
            .GetProperties();
        // var fieldNames = dClass.GetType()
        //     .get();

        foreach (var value in fieldValues)
        {
            if (value.Name == valName)
            {
                val = value;
                break;
            }
        }
    }

    public void GetVariableValue(TMP_Text txt)
    {
        txt.SetText(val.GetValue(reference.GetComponent(className)).ToString());
    }

    public void setInputFieldInt(TMP_Text txt)
    {
        val.SetValue(reference.GetComponent(className), int.Parse(txt.text));
    }
    public void setInputFieldFloat(TMP_Text txt)
    {
        val.SetValue(reference.GetComponent(className), float.Parse(txt.text));
    }
    public void setInputFieldString(TMP_Text txt)
    {
        val.SetValue(reference.GetComponent(className), txt.text);
    }

}