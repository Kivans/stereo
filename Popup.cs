using System.Collections;
using UnityEngine;

public class Popup : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        StartCoroutine(MovePopup());
	}

    private void Update()
    {
        if (transform.position.y >= 1300)
            Destroy(gameObject);
        else if (Input.GetKeyDown(KeyCode.Escape))
            Destroy(gameObject);
        
    }

    IEnumerator MovePopup()
    {
        while (gameObject.activeSelf)
        {
            GetComponent<Rigidbody2D>().AddForce(new Vector2(0, 105));
            yield return new WaitForSeconds(0.01f);
        }
    }
}
