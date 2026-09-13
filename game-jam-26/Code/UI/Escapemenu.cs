using Godot;

public partial class Escapemenu : Control
{
	private Control _startingMenu;
	private Control _gameOver;

	public override void _Ready()
	{
		ProcessMode = Node.ProcessModeEnum.Always;
		_startingMenu = GetNode<Control>("/root/Main/ScreenUI/StartingMenu");
		_gameOver = GetNode<Control>("/root/Main/ScreenUI/GameOver");
		Hide();
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is not InputEventKey key || !key.Pressed || key.Echo || key.Keycode != Key.Escape)
			return;
		if (_startingMenu.Visible || _gameOver.Visible)
			return;
		if (Visible)
			HideMenu();
		else
			ShowMenu();
	}

	public void ShowMenu()
	{
		GetTree().Paused = true;
		Show();
	}

	public void HideMenu()
	{
		GetTree().Paused = false;
		Hide();
	}

	private void OnReturnPressed()
	{
		HideMenu();
	}

	private void OnExitPressed()
	{
		GetTree().Quit(0);
	}
}
