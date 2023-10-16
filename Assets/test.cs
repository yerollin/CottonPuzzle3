using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

class DD
{
    static public void Log(string _log  )
    {
        string path = "Assets/Log.txt";
        if (File.Exists(path))
        {
            string log = File.ReadAllText(path);
            log += "\n"+_log + "  "+Time.time;
            File.WriteAllText(path,log);
        }
        else
        {
            File.WriteAllText(path, _log);
        }

    }


}

public class test : MonoBehaviour
{
    public class ListNode
    {
        public int val;
        public ListNode next;
        public ListNode(int val = 0, ListNode next = null)
        {
            this.val = val;
            this.next = next;
        }
    }

    public ListNode AddTwoNumbers(ListNode l1, ListNode l2)
    {
        var answer =new ListNode(0);
        ListNode iter = answer;

        bool Jin = false;
        for (int i = 0; i < 100; i++)
        {
            int tar = l1.val + l2.val + (Jin ? 1 : 0);
            if (tar > 9){
                tar = tar % 10;
                Jin = true;//进位
            }
            else Jin = false;

            var newd = new ListNode(tar,null);

            iter.next = newd;
            iter = newd;

            if (l1.next==l2.next )//双空，退出
            {
                if (Jin)
                    iter.next = new ListNode(1,null);
                break;
            }


            if (l1.next == null)
                l1.val = 0;

            if (l2.next == null)
                l2.val = 0;

        }

        return answer.next;
    }

    
 
    
    private void Awake()
    {
      
    }




}
