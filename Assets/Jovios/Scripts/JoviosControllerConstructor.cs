using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum JoviosControllerConstructorType{
	Joystick,
	Button,
	Label,
	Image,
	Avatar,
	Customize
}
public enum JoviosControlConstructorAlignment{
	Center,
	Left,
	Right
}

public class JoviosControllerConstructor : MonoBehaviour {

	public JoviosControlConstructorAlignment jcca;
	public JoviosControllerConstructorType jcct;
	public void AddControllerComponent(JoviosControllerStyle jcs){
		Vector2 positioning;
		float changeX = 0;
		float changeY = 0;
		string anchors;
		switch(jcca){
		case JoviosControlConstructorAlignment.Left:
			anchors = "ml";
			break;
		case JoviosControlConstructorAlignment.Right:
			anchors = "mr";
			break;
		case JoviosControlConstructorAlignment.Center:
			anchors = "mc";
			break;
		default:
			anchors = "mc";
			break;
		}
		positioning = new Vector2(transform.localPosition.x + changeX, transform.localPosition.y + changeY);
		switch(jcct){
		case JoviosControllerConstructorType.Button:
			jcs.AddButton1(positioning, new Vector2(transform.GetComponent<UIWidget>().width, transform.GetComponent<UIWidget>().height), anchors, transform.FindChild("Label").GetComponent<UILabel>().text, transform.name, color: transform.FindChild("Button1").GetComponent<UITexture>().color.ToString(), depth: transform.GetComponent<UIWidget>().depth, image: transform.FindChild("Button1").GetComponent<UITexture>().mainTexture.name);
			break;

		case JoviosControllerConstructorType.Customize:
			jcs.AddButton1(positioning, new Vector2(transform.GetComponent<UIWidget>().width, transform.GetComponent<UIWidget>().height), anchors, transform.FindChild("Label").GetComponent<UILabel>().text, "SPECIAL:CUSTOMIZE", color: transform.FindChild("Button1").GetComponent<UITexture>().color.ToString(), depth: transform.GetComponent<UIWidget>().depth, image: transform.FindChild("Button1").GetComponent<UITexture>().mainTexture.name);
			break;

		case JoviosControllerConstructorType.Joystick:
			jcs.AddJoystick(new Vector2(transform.localPosition.x, transform.localPosition.y), new Vector2(transform.GetComponent<UIWidget>().width, transform.GetComponent<UIWidget>().height), anchors, transform.name, depth: transform.GetComponent<UIWidget>().depth, joystickArrow: transform.FindChild("Arrow").GetComponent<UITexture>().mainTexture.name, joystickBackground: transform.FindChild("JoystickBackground").GetComponent<UITexture>().mainTexture.name, joystickBackdrop: transform.FindChild("Backdrop").GetComponent<UITexture>().mainTexture.name);
			break;
			
		case JoviosControllerConstructorType.Label:
			jcs.AddLabel(new Vector2(transform.localPosition.x, transform.localPosition.y), new Vector2(transform.GetComponent<UILabel>().width, transform.GetComponent<UILabel>().height), anchors, transform.GetComponent<UILabel>().text, color: transform.GetComponent<UILabel>().color.ToString(), depth: transform.GetComponent<UILabel>().depth, fontSize: transform.GetComponent<UILabel>().fontSize);
			break;
			
		case JoviosControllerConstructorType.Avatar:
			jcs.AddAvatar(new Vector2(transform.localPosition.x, transform.localPosition.y), new Vector2(transform.GetComponent<UIWidget>().width, transform.GetComponent<UIWidget>().height), anchors, depth: transform.GetComponent<UIWidget>().depth);
			break;
			
		case JoviosControllerConstructorType.Image:
			jcs.AddImage(new Vector2(transform.localPosition.x, transform.localPosition.y), new Vector2(transform.GetComponent<UILabel>().width, transform.GetComponent<UILabel>().height), anchors, transform.GetComponent<UITexture>().mainTexture.name, color: transform.GetComponent<UITexture>().color.ToString(), depth: transform.GetComponent<UILabel>().depth);
			break;
		}
	}
}