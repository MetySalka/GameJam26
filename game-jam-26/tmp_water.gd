extends SceneTree

func _initialize():
	for name in ["Beach_0001", "Beach_0004", "Beach.png"]:
		var img := Image.load_from_file("res://Assets/Sprites/Background/%s.png" % name)
		if img == null:
			print("[water] failed to load ", name)
			continue
		var w := img.get_width()
		var h := img.get_height()
		var first_blue := -1
		var last_row := []
		for y in range(h):
			var blue := 0
			for x in range(w):
				var c := img.get_pixel(x, y)
				if c.b > 0.40 and c.b > c.r + 0.10 and c.b > c.g + 0.02:
					blue += 1
			if blue > w / 2 and first_blue == -1:
				first_blue = y
			if y >= h - 8:
				last_row.append("%d:%d%%" % [y, roundi(100.0 * blue / w)])
		print("[water] ", name, " ", w, "x", h, " firstMajorityBlueRow=", first_blue,
			" fraction=", snappedf(float(first_blue) / float(h), 0.001), " tailRows=", last_row)
	quit()
