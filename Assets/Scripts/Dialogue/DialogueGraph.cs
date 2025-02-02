using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

[CreateAssetMenu]
public class DialogueGraph : NodeGraph
{

    public DialogueNode start;
    public DialogueNode current; //very similar to function declaration
    public DialogueNode initNode;

    public void Start()
    {
        start = initNode; //loops back to the start node
        current = initNode;
    }

}