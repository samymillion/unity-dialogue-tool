using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

[Serializable]
public struct Connection {}
public class DialogueNode : BaseNode
{
    [Input] public Connection entry;
    //[Output] public Connection exit;
    [Output(dynamicPortList = true)] public List<string> responseKeys;
    public string speakerName;
    [TextArea] public string promptKey;

    public override string GetString()
    {
        return "DialogueNode/" + speakerName + "/" + promptKey + "/" + responseKeys[0];
    }

    public override object GetValue(NodePort port)
    {
        return null;
    }



}
