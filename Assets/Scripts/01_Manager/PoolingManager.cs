using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 오브젝트 풀링 일반화
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
    /// 초기 풀 크기만큼 오브젝트를 생성하여 스택에 저장
    /// </summary>
    /// <param name="initialSize">풀 초기화 사이즈</param>
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
    /// 새로운 오브젝트를 생성하고 반환
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
    /// 오브젝트 풀에서 가져오기
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
            Debug.LogWarning($"[{typeof(T)}] 풀 부족 -> 새 오브젝트 생성.");
            return CreateNewObject();
        }    
    }

    /// <summary>
    /// 사용이 끝난 오브젝트를 풀로 반환
    /// </summary>
    /// <param name="obj"></param>
    public void ReturnObject(T obj)
    {
        obj.gameObject.SetActive(false);
        objectPool.Push(obj);
    }
}
