using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ������Ʈ Ǯ�� �Ϲ�ȭ
/// </summary>
/// <typeparam name="T"></typeparam>
public class PoolingManager<T> where T : MonoBehaviour
{
    private Stack<T> objectPool = new Stack<T>();
    private GameObject prefab;
    private Transform parentTransform;

    public PoolingManager(GameObject prefab, int initialSize, Transform parent)
    {
        this.prefab = prefab;
        this.parentTransform = parent;
        InitializePool(initialSize);
    }

    /// <summary>
    /// �ʱ� Ǯ ũ�⸸ŭ ������Ʈ�� �����Ͽ� ���ÿ� ����
    /// </summary>
    /// <param name="initialSize">Ǯ �ʱ�ȭ ������</param>
    private void InitializePool(int initialSize)
    {
        for(int i = 0; i < initialSize; i++)
        {
            T obj = CreateNewObject();
            obj.gameObject.SetActive(false);
            objectPool.Push(obj);
        }
    }

    /// <summary>
    /// ���ο� ������Ʈ�� �����ϰ� ��ȯ
    /// </summary>
    /// <returns></returns>
    private T CreateNewObject()
    {
        GameObject obj = Object.Instantiate(prefab);
        if(parentTransform)
        {
            obj.transform.SetParent(parentTransform);
        }
        return obj.GetComponent<T>();
    }

    /// <summary>
    /// ������Ʈ Ǯ���� ��������
    /// </summary>
    /// <returns></returns>
    public T GetObject()
    {
        if(objectPool.Count > 0)
        {
            T obj = objectPool.Pop();
            obj.gameObject.SetActive(true);
            return obj;
        }
        else
        {
            Debug.LogWarning($"[{typeof(T)}] Ǯ ���� -> �� ������Ʈ ����.");
            return CreateNewObject();
        }    
    }

    /// <summary>
    /// ����� ���� ������Ʈ�� Ǯ�� ��ȯ
    /// </summary>
    /// <param name="obj"></param>
    public void ReturnObject(T obj)
    {
        obj.gameObject.SetActive(false);
        objectPool.Push(obj);
    }
}
